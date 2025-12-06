using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddStartupCodeGenExtension
    {
        internal static void AddStartupCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, StartupCodeGen>();
        }
    }

    internal sealed class StartupCodeGen(ConsoleService consoleService) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using $namespace$.$dotNetToolName$;
                                        using Microsoft.Extensions.Configuration;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            internal sealed class Startup
                                            {
                                                internal void ConfigureServices(IServiceCollection services, 
                                                                                IConfiguration configuration)
                                                {
                                                    // 1. Infrastructure
                                                    services.Add$dotNetToolName$ArgumentFixer();
                                                    services.AddErrorHandler();

                                                    // 2. Domains
                                                    services.Add$dotNetToolName$CommandBuilder(configuration);
                                                }
                                            }
                                        }
                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add AppBuilder.cs
            var file = Path.Combine(projectFileInfo.Directory!.FullName, "Startup.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName);

            var formattedTemplate = newTemplate;

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 2. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
