using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;

namespace RunJit.Cli.New.RestMinimalApi
{
    internal static class AddStartupRegistrationVersionsExtension
    {
        internal static void AddStartupRegistrationVersions(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRestMinimalApiSpecificCodeGen, StartupRegistrationVersions>();
        }
    }

    internal sealed class StartupRegistrationVersions(ConsoleService consoleService) : IRestMinimalApiSpecificCodeGen
    {
        private const string Template = """
                                        $Usings$

                                        namespace $ProjectName$.Api.$DomainNamePlural$
                                        {
                                            internal static class Startup
                                            {
                                                internal static void Add$DomainNamePlural$(this IServiceCollection services, IConfiguration configuration)
                                                {
                                                    $ServiceRegistrations$
                                                }
                                            }
                                        }
                                        """;

        public async Task GenerateAsync(FileInfo solutionFileInfo,
                                        FileInfo webApiProject,
                                        CreateRestApiInfos createRestApiInfos)
        {
            // 1. Startup registration for domain versions
            var startupPath = Path.Combine(webApiProject.Directory!.FullName, "Api", createRestApiInfos.DomainNamePlural,
                                           "Startup.cs");

            var startupFileInfo = new FileInfo(startupPath);

            if (startupFileInfo.NotExists())
            {
                consoleService.WriteError($"Expected startup: {startupPath} to register new api endpoint does not exist");

                return;
            }

            //internal static class Startup
            //{
            //    internal static void AddProjects(this IServiceCollection services, IConfiguration configuration)
            //    {
            //        // Register your api domains here
            //        services.AddProjectsV1(configuration);
            //        services.AddProjectsV2(configuration);
            //    }

            //    internal static void MapProjects(this IEndpointRouteBuilder endpoints)
            //    {
            //        // Map your endpoints here
            //        endpoints.MapProjectsV1();
            //        endpoints.MapProjectsV2();
            //    }
            //}

            //using $ProjectName$.Api.$DomainNamePlural$.V$Version$;

            //namespace $ProjectName$.Api.$DomainNamePlural$
            //{
            //    internal static class Startup
            //    {
            //        internal static void Add$DomainNamePlural$(this IServiceCollection services, IConfiguration configuration)
            //        {
            //            services.Add$DomainNamePlural$V$Version$(configuration);
            //        }

            //        internal static void Map$DomainNamePlural$(this IEndpointRouteBuilder endpoints)
            //        {
            //            endpoints.Map$DomainNamePlural$V$Version$();
            //        }
            //    }
            //}

            // 2. Version folders
            var versions = startupFileInfo.Directory!.EnumerateDirectories().Select(d => d.Name).ToList();

            var usings = versions.Select(version => $"using {createRestApiInfos.ProjectName}.Api.{createRestApiInfos.DomainNamePlural}.{version};").Flatten(Environment.NewLine);
            var serviceRegistrations = versions.Select(version => $"services.Add{createRestApiInfos.DomainNamePlural}{version}(configuration);").Flatten(Environment.NewLine);
            var endpointMappings = versions.Select(version => $"endpoints.Map{createRestApiInfos.DomainNamePlural}{version}();").Flatten(Environment.NewLine);

            var startup = Template.Replace("$ProjectName$", createRestApiInfos.ProjectName)
                                  .Replace("$DomainNamePlural$", createRestApiInfos.DomainNamePlural)
                                  .Replace("$Usings$", usings)
                                  .Replace("$ServiceRegistrations$", serviceRegistrations)
                                  .Replace("$EndpointMappings$", endpointMappings)
                                  .FormatSyntaxTree();

            await File.WriteAllTextAsync(startupFileInfo.FullName, startup).ConfigureAwait(false);
        }
    }
}
