using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.New.NugetProject;
using RunJit.Cli.Services;

namespace RunJit.Cli.New.NugetProject
{
    internal static class AddNewNugetProjectServiceExtension
    {
        internal static void AddNewNugetProjectService(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddProcessService();
            services.AddNugetProjectCreator();
            services.AddNugetProjectContractsGenerator();
            
            services.AddSingletonIfNotExists<NewNugetProjectService>();
        }
    }

    internal sealed class NewNugetProjectService(ConsoleService consoleService,
                                                NugetProjectCreator newNugetProjectService)
    {
        public async Task<int> HandleAsync(NewNugetProjectParameters parameters)
        {
            //// ToDo: Idea a new parameter to control with or without build :)
            //// 0. Build the target solution first
            //if (parameters.Build)
            //{
            //    var dotnetBuildResult = await processService.RunAsync("dotnet", $"build {parameters.SolutionFile.FullName}").ConfigureAwait(false);

            //    if (dotnetBuildResult.ExitCode != 0)
            //    {
            //        return dotnetBuildResult.ExitCode;
            //    }
            //}

            var newParameters = parameters with
            {
                TargetDirectoryInfo = parameters.TargetDirectoryInfo ?? new DirectoryInfo(Environment.CurrentDirectory),
                TargetFramework = parameters.TargetFramework < 9 ? 9 : parameters.TargetFramework
            };

            // 1. Build client generator from parameters
            var projectName = $"{parameters.ProjectName}";

            // 2. Generate the dotnet tool into solution
            var solutionFileInfo = await newNugetProjectService.GenerateProjectAsync(newParameters).ConfigureAwait(false);

            // 3. Write success message
            consoleService.WriteSuccess($"Enjoy your new generated: '{projectName}' .net tool");

            // 4. Write solution file info
            consoleService.WriteSuccess(solutionFileInfo.FullName);

            return 0;
        }
    }
}
