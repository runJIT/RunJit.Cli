using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using Solution.Parser.Solution;

namespace RunJit.Cli.New.NugetProject
{
    internal interface INugetProjectTestCaseCodeGen
    {
        Task<string> GenerateAsync(NugetProjectInfos minimalApiProjectInfos,
                                   string cliCallPath);
    }

    internal interface INugetProjectRootLevelCodeGen
    {
        Task GenerateAsync(SolutionFile solutionFile,
                           NugetProjectInfos minimalApiProjectInfos);
    }

    internal interface INugetProjectSpecificCodeGen
    {
        Task GenerateAsync(FileInfo projectFileInfo,
                           FileInfo solutionFile,
                           XDocument projectDocument,
                           NugetProjectInfos minimalApiProjectInfos);
    }

    internal interface INugetProjectContractsSpecificCodeGen
    {
        Task GenerateAsync(FileInfo projectFileInfo,
                           FileInfo solutionFile,
                           XDocument projectDocument,
                           NugetProjectInfos minimalApiProjectInfos);
    }

    internal interface INugetProjectTestSpecificCodeGen
    {
        Task GenerateAsync(FileInfo projectFileInfo,
                           FileInfo solutionFile,
                           XDocument projectDocument,
                           NugetProjectInfos minimalApiProjectInfos);
    }

    internal static class AddNugetProjectCodeGenExtension
    {
        internal static void AddNugetProjectCodeGen(this IServiceCollection services)
        {
            services.AddNugetProjectGenerator();
            services.AddNugetProjectTestGenerator();

            services.AddSingletonIfNotExists<NugetProjectsCodeGen>();
        }
    }

    internal sealed class NugetProjectsCodeGen(NugetProjectContractsGenerator nugetProjectContractsGenerator,
                                               NugetProjectGenerator nugetProjectGenerator,
                                               NugetProjectTestGenerator nugetProjectTestGenerator,
                                               IEnumerable<INugetProjectRootLevelCodeGen> rootLevelCodeGens)
    {
        internal async Task GenerateAsync(SolutionFile solutionFile,
                                          NugetProjectInfos minimalApiProjectInfos)
        {
            // 1. Write all root level code
            foreach (var minimalApiProjectRootLevelCodeGen in rootLevelCodeGens)
            {
                await minimalApiProjectRootLevelCodeGen.GenerateAsync(solutionFile, minimalApiProjectInfos).ConfigureAwait(false);
            }

            // 2. Create contracts project first, because the implementation package needs the reference to the contracts
            var contractProject = await nugetProjectContractsGenerator.GenerateAsync(solutionFile, minimalApiProjectInfos).ConfigureAwait(false);

            // 3. Create the new web api project    
            var libraryProject = await nugetProjectGenerator.GenerateAsync(solutionFile, contractProject, minimalApiProjectInfos).ConfigureAwait(false);

            // 4. Create the test project for the new web api project    
            await nugetProjectTestGenerator.GenerateAsync(solutionFile, libraryProject, minimalApiProjectInfos).ConfigureAwait(false);
        }
    }
}
