using System.CommandLine.Invocation;
using System.Text;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.RunJit.Update.Nuget;

namespace RunJit.Cli.Services.Net
{
    internal static class AddDotNetExtension
    {
        internal static void AddDotNet(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNet, DotNet>();
        }
    }

    internal interface IDotNet
    {
        Task<OutdatedNugetResponse> ListOutdatedPackagesAsync(FileInfo solutionFile);

        Task AddNugetPackageAsync(string projectPath,
                                  string packageId,
                                  string packageVersion);

        Task BuildAsync(FileInfo solutionFileOrProject);

        Task<TryResult> TryBuildAsync(FileInfo solutionFileOrProject);

        Task RestoreNugetPackagesAsync(FileInfo solutionFileOrProject);

        Task AddProjectToSolutionAsync(FileInfo solutionFileInfo,
                                       FileInfo projectFileInfo,
                                       string solutionFolder = "");

        Task RemoveProjectFromSolutionAsync(FileInfo solutionFileInfo,
                                            FileInfo projectFileInfo);

        Task AddProjectReference(FileInfo projectReference,
                                 FileInfo projectFileInfo);

        Task RemoveProjectReference(FileInfo projectReference,
                                    FileInfo projectFileInfo);

        Task RunAsync(string command,
                      string arguments,
                      string successMessage = "",
                      string errorTitle = "");
    }

    internal sealed record TryResult(bool WasSuccessful,
                                     string Message);

    internal sealed class DotNet(ConsoleService consoleService) : IDotNet
    {
        public async Task<OutdatedNugetResponse> ListOutdatedPackagesAsync(FileInfo solutionFile)
        {
            consoleService.WriteInfo($"dotnet list  {solutionFile.FullName} package --format json --outdated");

            var stringBuilder = new StringBuilder();

            var outdatedPackageCommand = Process.StartProcess("dotnet", $"dotnet list  {solutionFile.FullName} package --format json --outdated", null,
                                                              o => stringBuilder.AppendLine(o));

            await outdatedPackageCommand.WaitForExitAsync().ConfigureAwait(false);

            var output = stringBuilder.ToString();

            // var outdatedPackageCommand = await dotNetTool.RunAsync("dotnet", $"dotnet list  {solutionFile.FullName} package --format json --outdated").ConfigureAwait(false);
            if (outdatedPackageCommand.ExitCode != 0)
            {
                throw new RunJitException($"Could not get outdated packages for solution file: {solutionFile.FullName}{Environment.NewLine}{output}");
            }

            var splittedOutput = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var jsonStart = splittedOutput.FirstOrDefault(item => item[0].EqualsTo('{'));
            var index = splittedOutput.IndexOf(jsonStart);
            var jsonOnly = string.Join(Environment.NewLine, splittedOutput.Skip(index));

            var outdatedNugetResponse = jsonOnly.FromJsonStringAs<OutdatedNugetResponse>();

            return outdatedNugetResponse;
        }

        public Task AddNugetPackageAsync(string projectPath,
                                         string packageId,
                                         string packageVersion)
        {
            return RunAsync("dotnet",
                            $"add {projectPath} package {packageId} --version {packageVersion}",
                            $"Add nuget package: {packageId} with version: {packageVersion} into project: {projectPath} successful",
                            $"Add nuget package: {packageId} with version: {packageVersion} into project: {projectPath} failed.");
        }

        public Task BuildAsync(FileInfo solutionFileOrProject)
        {
            return RunAsync("dotnet",
                            $"build {solutionFileOrProject.FullName}",
                            $"Build solution or project: {solutionFileOrProject} successful",
                            $"Could not build solution file or project: {solutionFileOrProject.FullName}");
        }

        public async Task<TryResult> TryBuildAsync(FileInfo solutionFileOrProject)
        {
            consoleService.WriteInfo($"Build solution or project: {solutionFileOrProject}");
            var stringBuilder = new StringBuilder();

            var process = Process.StartProcess("dotnet", $"build {solutionFileOrProject.FullName}", null,
                                               o => stringBuilder.AppendLine(o));

            await process.WaitForExitAsync().ConfigureAwait(false);

            var output = stringBuilder.ToString();

            // var buildResult = await dotNetTool.RunAsync("dotnet", $"build {solutionFileOrProject.FullName}").ConfigureAwait(false);
            if (process.ExitCode != 0)
            {
                consoleService.WriteError($"Build solution or project: {solutionFileOrProject} was not successful");

                return new TryResult(false, output);
            }

            consoleService.WriteInfo($"Build solution or project: {solutionFileOrProject} successful");

            return new TryResult(true, output);
        }

        public Task RestoreNugetPackagesAsync(FileInfo solutionFileOrProject)
        {
            return RunAsync("dotnet",
                            $"restore {solutionFileOrProject.FullName}",
                            $"Restore nuget packages for: {solutionFileOrProject} successful",
                            $"Could not restore nuget packages for: {solutionFileOrProject.FullName}{Environment.NewLine}");
        }

        public async Task AddProjectToSolutionAsync(FileInfo solutionFileInfo,
                                                    FileInfo projectFileInfo,
                                                    string solutionFolder = "")
        {
            var solutionFolderParameter = solutionFolder.IsNullOrWhiteSpace() ? "--in-root" : $"--solution-folder {solutionFolder}";

            await RunAsync("dotnet",
                           $"sln {solutionFileInfo.FullName} add {projectFileInfo.FullName} {solutionFolderParameter}",
                           $"Add project: {projectFileInfo.FullName} to solution: {solutionFileInfo.FullName} successful",
                           $"Add project: {projectFileInfo.FullName} to solution: {solutionFileInfo.FullName} failed.").ConfigureAwait(false);
        }

        public async Task AddFileIntoSolutionFolderAsync(FileInfo solutionFileInfo,
                                                         FileInfo fileToAdd,
                                                         string solutionFolder = "")
        {
            var solutionFolderParameter = solutionFolder.IsNullOrWhiteSpace() ? "--in-root" : $"--solution-folder {solutionFolder}";

            await RunAsync("dotnet",
                           $"sln {solutionFileInfo.FullName} add-file {fileToAdd.FullName} {solutionFolderParameter}",
                           $"Add file: {fileToAdd.FullName} to solution: {solutionFileInfo.FullName} successful",
                           $"Add file: {fileToAdd.FullName} to solution: {solutionFileInfo.FullName} failed.").ConfigureAwait(false);

            throw new NotImplementedException();
        }

        public Task RemoveProjectFromSolutionAsync(FileInfo solutionFileInfo,
                                                   FileInfo projectFileInfo)
        {
            return RunAsync("dotnet",
                            $"sln {solutionFileInfo.FullName} remove {projectFileInfo.FullName} --in-root",
                            $"Remove project: {projectFileInfo.FullName} to solution: {solutionFileInfo.FullName} successful",
                            $"Remove project: {projectFileInfo.FullName} to solution: {solutionFileInfo.FullName} failed.");
        }

        public Task AddProjectReference(FileInfo projectFileInfo,
                                        FileInfo projectReference)
        {
            return RunAsync("dotnet",
                            $"add {projectReference.FullName} reference {projectFileInfo.FullName}",
                            $"Add project reference: {projectReference.FullName} from project: {projectFileInfo.FullName} was successful.",
                            $"Add project reference: {projectReference.FullName} from project: {projectFileInfo.FullName} failed.");
        }

        public Task RemoveProjectReference(FileInfo projectFileInfo,
                                           FileInfo projectReference)
        {
            return RunAsync("dotnet",
                            $"remove {projectReference.FullName} reference {projectFileInfo.FullName}",
                            $"Remove project reference: {projectReference.FullName} from project: {projectFileInfo.FullName} was successful.",
                            $"Remove project reference: {projectReference.FullName} from project: {projectFileInfo.FullName} failed.");
        }

        public async Task RunAsync(string command,
                                   string arguments,
                                   string successMessage = "",
                                   string errorTitle = "")
        {
            consoleService.WriteInfo($"Run custom command: {command} {arguments}");

            var stringBuilder = new StringBuilder();
            var errorStringBuilder = new StringBuilder();

            var buildResult = Process.StartProcess(command, arguments, null,
                                                   std => stringBuilder.AppendLine(std), error => errorStringBuilder.AppendLine(error));

            await buildResult.WaitForExitAsync().ConfigureAwait(false);

            if (buildResult.ExitCode != 0)
            {
                var normalOutput = stringBuilder.ToString();
                var errorOutput = errorStringBuilder.ToString();

                var output = errorOutput.IsNotNullOrWhiteSpace() ? errorOutput : normalOutput;

                var errorMessage = errorTitle.IsNotNullOrWhiteSpace() ? $"{errorTitle}. {output}" : $"Run custom command: {command} {arguments} successful failed. {output}";

                throw new RunJitException(errorMessage);
            }

            var successInfo = successMessage.IsNotNullOrWhiteSpace() ? successMessage : $"Run custom command: {command} {arguments} successful";
            consoleService.WriteSuccess(successInfo);
        }
    }
}
