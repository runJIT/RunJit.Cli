using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Generate.DotNetTool;
using RunJit.Cli.RunJit.Generate.Client;
using RunJit.Cli.Services;
using RunJit.Cli.Services.Net;
using RunJit.Cli.Services.Resharper;
using Solution.Parser.Solution;

namespace RunJit.Cli.New.NugetProject
{
    internal static class AddNugetProjectCreatorExtension
    {
        internal static void AddNugetProjectCreator(this IServiceCollection services)
        {
            services.AddControllerParser();
            services.AddApiTypeLoader();
            services.AddRestructureController();
            services.AddEndpointsToCliCommandStructureConverter();
            services.AddMinimalApiEndpointParser();
            services.AddOrganizeMinimalEndpoints();
            services.AddDotNetToolCodeGen();
            services.AddSolutionCodeCleanup();
            services.AddIDEService();

            services.AddSingletonIfNotExists<NugetProjectCreator>();
        }
    }

    internal sealed class NugetProjectCreator(NugetProjectsCodeGen minimalApiProjectsCodeGen,
                                            IDotNet dotNet,
                                            IDEService ideService)

    {
        internal async Task<FileInfo> GenerateProjectAsync(NewNugetProjectParameters newNugetProjectParameters)
        {
            // 1. Generate empty solution
            var solutionFileName = newNugetProjectParameters.ProjectName.EndsWith(".sln") ? newNugetProjectParameters.ProjectName : $"{newNugetProjectParameters.ProjectName}.sln";

            // 2. Eval target directory
            if (newNugetProjectParameters.TargetDirectoryInfo.Exists)
            {
                newNugetProjectParameters.TargetDirectoryInfo.Delete(true);
            }

            // 3. Create target directory
            newNugetProjectParameters.TargetDirectoryInfo.Create();

            // 4. Create new solution
            await dotNet.RunAsync("dotnet", $"dotnet new sln --name {newNugetProjectParameters.ProjectName} --output {newNugetProjectParameters.TargetDirectoryInfo.FullName}").ConfigureAwait(false);

            // 5. Check if solution was created
            var targetSolutionFile = new FileInfo(Path.Combine(newNugetProjectParameters.TargetDirectoryInfo.FullName, solutionFileName));
            if (targetSolutionFile.NotExists())
            {
                throw new RunJitException($"The target solution: {targetSolutionFile.FullName} was not found please check if the Web-Api solution was successfully created and if so, check the path of the creation.");
            }

            // 6. Parse solution and setup project infos
            var parsedSolution = new SolutionFileInfo(targetSolutionFile.FullName).Parse();
            var minimalApiProjectInfos = new NugetProjectInfos
            {
                ProjectName = newNugetProjectParameters.ProjectName,
                ContractProjectName = $"{newNugetProjectParameters.ProjectName}.Contracts",
                NetVersion = newNugetProjectParameters.TargetFramework < 9 ? $"net9.0" : $"net{newNugetProjectParameters.TargetFramework}.0",
                Name = newNugetProjectParameters.ProjectName,
                NormalizedName = newNugetProjectParameters.ProjectName,
                RepoName = newNugetProjectParameters.ProjectName.Split(".").Select(part => part.ToLower()).Flatten("-")
            };

            // 7. Run all code generators
            await minimalApiProjectsCodeGen.GenerateAsync(parsedSolution, minimalApiProjectInfos).ConfigureAwait(false);

            // 8. Cleanup code
            // await solutionCodeCleanup.CleanupSolutionAsync(targetSolutionFile).ConfigureAwait(false);

            // 9. Open solution in IDE
            if (newNugetProjectParameters.StartIde)
            {
                // ideService.LaunchIdeFireAndForget(parsedSolution.SolutionFileInfo.Value);
                await ideService.LaunchIdeAsync(parsedSolution.SolutionFileInfo.Value).ConfigureAwait(false);    
            }

            return parsedSolution.SolutionFileInfo.Value;
        }
    }
}
