using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using Solution.Parser.Solution;

namespace RunJit.Cli.Services
{
    internal static class AddApiTypeLoaderExtension
    {
        internal static void AddApiTypeLoader(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ApiTypeLoader>();
        }
    }

    internal sealed class ApiTypeLoader(AssemblyTypeLoader assemblyTypeLoader)
    {
        internal ImmutableList<Type> GetAllTypesFrom(SolutionFile parsedSolution)
        {
            var webAppProject = parsedSolution.ProductiveProjects.FirstOrDefault(p => p.Document.ToString().Contains("Sdk=\"Microsoft.NET.Sdk.Web\""));

            if (webAppProject.IsNull())
            {
                throw new RunJitException($"Your solution: {parsedSolution.SolutionFileInfo.Value} does not contain a project which is defined as Sdk=\"Microsoft.NET.Sdk.Web\"");
            }

            var searchPattern = $"{webAppProject.ProjectFileInfo.FileNameWithoutExtenion}.dll";

            var assembly = webAppProject.ProjectFileInfo.Value.Directory!.EnumerateFiles(searchPattern, SearchOption.AllDirectories)
                                        .FirstOrDefault(file => file.FullName.Contains("Debug") &&
                                                                (file.FullName.Contains("net8") || file.FullName.Contains("net9")) &&
                                                                !file.FullName.Contains("obj"));

            if (assembly.IsNull())
            {
                throw new RunJitException($"Your project: '{webAppProject.ProjectFileInfo.Value.FullName}' path does not contain the matching assembly: '{assembly}'. Please build your project or solution before you want to create a client from");
            }

            // Get all types which are declared in the API assembly - Need to unique ident the types for client generation.
            var types = assemblyTypeLoader.GetAllTypesFrom(assembly);

            return types;
        }
    }
}
