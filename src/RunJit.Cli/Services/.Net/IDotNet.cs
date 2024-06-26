﻿using System.CommandLine.Invocation;
using System.Text;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.RunJit.Update.Nuget;

namespace RunJit.Cli.Net
{

    public static class AddDotNetExtension
    {
        public static void AddDotNet(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNet, DotNet>();
        }
    }

    internal interface IDotNet
    {
        Task<OutdatedNugetResponse> ListOutdatedPackagesAsync(FileInfo solutionFile);
        Task AddNugetPackageAsync(string projectPath, string packageId, string packageVersion);
        Task BuildAsync(FileInfo solutionFileOrProject);
        Task<TryResult> TryBuildAsync(FileInfo solutionFileOrProject);
        Task RestoreNugetPackagesAsync(FileInfo solutionFileOrProject);
        Task AddProjectToSolutionAsync(FileInfo solutionFileInfo, FileInfo projectFileInfo);
        Task RunAsync(string command, string arguments);

    }

    internal record TryResult(bool WasSuccessful, string Message);

    internal class DotNet(IConsoleService consoleService) : IDotNet
    {
        public async Task<OutdatedNugetResponse> ListOutdatedPackagesAsync(FileInfo solutionFile)
        {
            consoleService.WriteInfo($"dotnet list  {solutionFile.FullName} package --format json --outdated");

            var stringBuilder = new StringBuilder();
            var outdatedPackageCommand = Process.StartProcess("dotnet", $"dotnet list  {solutionFile.FullName} package --format json --outdated", null, o => stringBuilder.AppendLine(o));
            await outdatedPackageCommand.WaitForExitAsync().ConfigureAwait(false);

            var output = stringBuilder.ToString();
            // var outdatedPackageCommand = await dotNetTool.RunAsync("dotnet", $"dotnet list  {solutionFile.FullName} package --format json --outdated").ConfigureAwait(false);
            if (outdatedPackageCommand.ExitCode != 0)
            {
                throw new RunJitException($"Could not get outdated packages for solution file: {solutionFile.FullName}{Environment.NewLine}{output}");
            }

            var splittedOutput = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var jsonStart = splittedOutput.FirstOrDefault(item => item[0] == '{');
            var index = splittedOutput.IndexOf(jsonStart);
            var jsonOnly = string.Join(Environment.NewLine, splittedOutput.Skip(index));
            
            var outdatedNugetResponse = jsonOnly.FromJsonStringAs<OutdatedNugetResponse>();
            return outdatedNugetResponse;
        }



        public async Task AddNugetPackageAsync(string projectPath, string packageId, string packageVersion)
        {
            consoleService.WriteInfo($"Add nuget package: {packageId} with version: {packageVersion} into project: {projectPath}");

            var process = Process.StartProcess("dotnet", $"add {projectPath} package {packageId} --version {packageVersion}");
            await process.WaitForExitAsync().ConfigureAwait(false);

            // var buildResult = await dotNetTool.RunAsync("dotnet", $"build {solutionFileOrProject.FullName}").ConfigureAwait(false);
            if (process.ExitCode != 0)
            {
                throw new RunJitException($"Add nuget package: {packageId} with version: {packageVersion} into project: {projectPath} failed.");
            }

            consoleService.WriteSuccess($"Add nuget package: {packageId} with version: {packageVersion} into project: {projectPath} successful");
        }

        public async Task BuildAsync(FileInfo solutionFileOrProject)
        {
            consoleService.WriteInfo($"Build solution or project: {solutionFileOrProject}");

            var process = Process.StartProcess("dotnet", $"build {solutionFileOrProject.FullName}");
            await process.WaitForExitAsync().ConfigureAwait(false);

            // var buildResult = await dotNetTool.RunAsync("dotnet", $"build {solutionFileOrProject.FullName}").ConfigureAwait(false);
            if (process.ExitCode != 0)
            {
                throw new RunJitException($"Could not build solution file or project: {solutionFileOrProject.FullName}");
            }
            consoleService.WriteInfo($"Build solution or project: {solutionFileOrProject} successful");
        }

        public async Task<TryResult> TryBuildAsync(FileInfo solutionFileOrProject)
        {
            consoleService.WriteInfo($"Build solution or project: {solutionFileOrProject}");
            var stringBuilder = new StringBuilder();

            var process = Process.StartProcess("dotnet", $"build {solutionFileOrProject.FullName}", null, o => stringBuilder.AppendLine(o));
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

        public async Task RestoreNugetPackagesAsync(FileInfo solutionFileOrProject)
        {
            consoleService.WriteInfo($"Restore nuget packages for: {solutionFileOrProject}");

            var buildResult = Process.StartProcess("dotnet", $"restore {solutionFileOrProject.FullName}");
            await buildResult.WaitForExitAsync().ConfigureAwait(false);

            // var buildResult = await dotNetTool.RunAsync("dotnet", $"restore {solutionFileOrProject.FullName}").ConfigureAwait(false);
            if (buildResult.ExitCode != 0)
            {
                throw new RunJitException($"Could not restore nuget packages for: {solutionFileOrProject.FullName}{Environment.NewLine}");
            }
            consoleService.WriteInfo($"Restore nuget packages for: {solutionFileOrProject} successful");
        }

        public async Task AddProjectToSolutionAsync(FileInfo solutionFileInfo, FileInfo projectFileInfo)
        {
            consoleService.WriteInfo($"Add project: {projectFileInfo.FullName} to solution: {solutionFileInfo.FullName}");

            var buildResult = Process.StartProcess("dotnet", $"sln {solutionFileInfo.FullName} add {projectFileInfo.FullName} --in-root");
            await buildResult.WaitForExitAsync().ConfigureAwait(false);

            // var addProjectResult = await dotNetTool.RunAsync("dotnet", $"sln {solutionFileInfo.FullName} add {projectFileInfo.FullName} --in-root");
            if (buildResult.ExitCode != 0)
            {
                throw new RunJitException($"Add project: {projectFileInfo.FullName} to solution: {solutionFileInfo.FullName} failed.");
            }

            consoleService.WriteSuccess($"Add project: {projectFileInfo.FullName} to solution: {solutionFileInfo.FullName} successful");
        }

        public async Task RunAsync(string command, string arguments)
        {
            consoleService.WriteInfo($"Run custom command: {command} {arguments}");

            var stringBuilder = new StringBuilder();
            var errorStringBuilder = new StringBuilder();

            var buildResult = Process.StartProcess(command, arguments, null, std => stringBuilder.AppendLine(std), error => errorStringBuilder.AppendLine(error));
            await buildResult.WaitForExitAsync().ConfigureAwait(false);

            if (buildResult.ExitCode != 0)
            {
                var output = stringBuilder.ToString();
                throw new RunJitException($"Run custom command: {command} {arguments} successful failed. {output}");
            }

            consoleService.WriteSuccess($"Run custom command: {command} {arguments} successful");
        }
    }
}
