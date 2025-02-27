using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;

namespace RunJit.Cli.New.RestMinimalApi
{
    internal static class AddStartupRegistrationExtension
    {
        internal static void AddStartupRegistration(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRestMinimalApiSpecificCodeGen, StartupRegistration>();
        }
    }

    internal sealed class StartupRegistration(ConsoleService consoleService) : IRestMinimalApiSpecificCodeGen
    {
        public async Task GenerateAsync(FileInfo solutionFileInfo,
                                        FileInfo webApiProject,
                                        CreateRestApiInfos createRestApiInfos)
        {
            // We have to add the new api into api startup
            var startupPath = Path.Combine(webApiProject.Directory!.FullName, "Api", "Startup.cs");
            var startupFileInfo = new FileInfo(startupPath);

            if (startupFileInfo.NotExists())
            {
                consoleService.WriteError($"Expected startup: {startupPath} to register new api endpoint does not exist");
                return;
            }

            //internal static class Startup
            //{
            //    internal static void AddApi(this IServiceCollection services, IConfiguration configuration)
            //    {
            //        services.AddProjects(configuration);
            //        // Register your api domains here
            //    }

            //    internal static void MapApi(this IEndpointRouteBuilder endpoints)
            //    {
            //        endpoints.MapProjects();
            //        // Map your endpoints here
            //    }
            //}

            var syntaxTree = new CSharpFileInfo(startupFileInfo.FullName).Parse();
            var newSyntaxTree = syntaxTree.SyntaxTree;
            var methods = syntaxTree.Classes.SelectMany(c => c.Methods).ToList();

            var addApiMethod = methods.FirstOrDefault(m => m.Name == "AddApi");
            if (addApiMethod.IsNotNull())
            {
                var newLineStatements = addApiMethod.LineStatements.Add($"services.Add{createRestApiInfos.DomainNamePlural}(configuration);");
                var orgLines = addApiMethod.LineStatements.ToFlattenString(Environment.NewLine);
                var flattenString = newLineStatements.ToFlattenString(Environment.NewLine);

                newSyntaxTree = newSyntaxTree.Replace(orgLines, flattenString);
            }

            var mapMethod = methods.FirstOrDefault(m => m.Name == "MapApi");
            if (mapMethod.IsNotNull())
            {
                var newLineStatements = mapMethod.LineStatements.Add($"endpoints.Map{createRestApiInfos.DomainNamePlural}();");
                var orgLines = mapMethod.LineStatements.ToFlattenString(Environment.NewLine);
                var flattenString = newLineStatements.ToFlattenString(Environment.NewLine);

                newSyntaxTree = newSyntaxTree.Replace(orgLines, flattenString);
            }

            var originalUsings = syntaxTree.Usings.Select(u => u.Value).ToList();
            var newUsings = originalUsings.Concat($"using {createRestApiInfos.ProjectName}.Api.{createRestApiInfos.DomainNamePlural};").ToFlattenString(Environment.NewLine);

            if (originalUsings.IsEmpty())
            {
                newSyntaxTree = $"{newUsings}{Environment.NewLine}{Environment.NewLine}{newSyntaxTree}";
            }
            else
            {
                newSyntaxTree = newSyntaxTree.Replace(originalUsings.ToFlattenString(Environment.NewLine), newUsings);    
            }
            
            
            
            //internal static class Startup
            //{
            //    internal static void AddApi(this IServiceCollection __,
            //                                IConfiguration _)
            //    {
            //        // Register your api domains here
            //        services.AddProjects(configuration);
            //    }

            //    internal static void MapApi(this IEndpointRouteBuilder _)
            //    {
            //        // Map your endpoints here
            //        endpoints.MapProjects(configuration);
            //    }
            //}

            newSyntaxTree = newSyntaxTree.Replace("IServiceCollection __", "IServiceCollection services")
                                         .Replace("IConfiguration _", "IConfiguration configuration")
                                         .Replace("IEndpointRouteBuilder _", "IEndpointRouteBuilder endpoints");
                                            
            await File.WriteAllTextAsync(startupFileInfo.FullName, newSyntaxTree).ConfigureAwait(false);
        }
    }
}
