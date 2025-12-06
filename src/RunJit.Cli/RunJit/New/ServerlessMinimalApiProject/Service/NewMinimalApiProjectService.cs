using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool;
using RunJit.Cli.Services;

namespace RunJit.Cli.New.MinimalApiProject
{
    internal static class AddNewMinimalApiProjectServiceExtension
    {
        internal static void AddNewMinimalApiProjectService(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddProcessService();
            services.AddMinimalApiProjectCreator();

            services.AddSingletonIfNotExists<NewMinimalApiProjectService>();
        }
    }

    internal sealed class NewMinimalApiProjectService(ConsoleService consoleService,
                                                      MinimalApiProjectCreator newMinimalApiProjectService)
    {
        public async Task<int> HandleAsync(NewMinimalApiProjectParameters parameters)
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
            var solutionFileInfo = await newMinimalApiProjectService.GenerateProjectAsync(newParameters).ConfigureAwait(false);

            // 3. Write success message
            consoleService.WriteSuccess($"Enjoy your new generated: '{projectName}' .net tool");

            // 4. Write solution file info
            consoleService.WriteSuccess(solutionFileInfo.FullName);

            return 0;
        }
    }
}
