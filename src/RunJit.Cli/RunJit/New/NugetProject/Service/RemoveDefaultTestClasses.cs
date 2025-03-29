using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.New.NugetProject
{
    internal static class AddRemoveDefaultTestClassesExtension
    {
        internal static void AddRemoveDefaultTestClasses(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<INugetProjectTestSpecificCodeGen, RemoveDefaultTestClasses>();
        }
    }

    // Visual studio creates default test class and test settings -> we dont want that from
    // beginning
    internal sealed class RemoveDefaultTestClasses(ConsoleService consoleService) : INugetProjectTestSpecificCodeGen
    {
        public Task GenerateAsync(FileInfo projectFileInfo,
                                  FileInfo solutionFile,
                                  XDocument projectDocument,
                                  NugetProjectInfos minimalApiProjectInfos)
        {
            // Test1
            // MSTestSettings.cs
            foreach (var file in projectFileInfo.Directory!.EnumerateFiles("*.cs"))
            {
                if (file.Name.Contains("Test1", StringComparison.OrdinalIgnoreCase))
                {
                    file.Delete();
                }

                if (file.Name.Contains("MsTestSettings", StringComparison.OrdinalIgnoreCase))
                {
                    file.Delete();
                }
            }

            consoleService.WriteSuccess("Successfully removed default test file created by .net cli");

            return Task.CompletedTask;
        }
    }
}
