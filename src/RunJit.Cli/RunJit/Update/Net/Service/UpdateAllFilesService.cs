using System.Text.RegularExpressions;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using RunJit.Cli.Services;

namespace RunJit.Cli.Update
{
    internal static class AddUpdateAllFilesServiceExtension
    {
        internal static void AddUpdateAllFilesService(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddUpdateDotNetVersionParameters();
            services.AddFindSolutionFile();

            services.AddSingletonIfNotExists<UpdateAllFilesService>();
        }
    }

    internal sealed class UpdateAllFilesService(ConsoleService consoleService,
                                                FindSolutionFile findSolutionFile)
    {
        private readonly Regex _netVersionReplaceRegex = new(@"net\d+\.\d+", RegexOptions.Compiled);

        private readonly Regex _versionReplaceRegex = new(@"(\d+\.\d-+)", RegexOptions.Compiled);

        public async Task HandleAsync(UpdateDotNetVersionParameters versionParameters)
        {
            // Just POC :) 

            // 1. Check if solution file is the file or directory
            //    if it is null or whitespace we check current directory
            var solutionFile = findSolutionFile.Find(versionParameters.SolutionFile);

            // 2. Update docker file if exists
            var dockerFiles = solutionFile.Directory!.EnumerateFiles("Dockerfile");

            foreach (var dockerFile in dockerFiles)
            {
                var fileContent = await File.ReadAllTextAsync(dockerFile.FullName).ConfigureAwait(false);

                // FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
                var newfileContent = _versionReplaceRegex.Replace(fileContent, $"{versionParameters.Version}.0-");
                await File.WriteAllTextAsync(dockerFile.FullName, newfileContent).ConfigureAwait(false);
            }

            // 3. Update yml files if exists

            var ymlFiles = solutionFile.Directory!.EnumerateFiles("*.yml", SearchOption.AllDirectories).Where(f => f.FullName.DoesNotContain(".git"));

            foreach (var ymlFile in ymlFiles)
            {
                var ymlFileContent = await File.ReadAllTextAsync(ymlFile.FullName).ConfigureAwait(false);

                // FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
                var newfileContent = Regex.Replace(ymlFileContent, @"--channel \d+\.\d+", $"--channel {versionParameters.Version}.0");

                if (ymlFileContent.NotEqualsTo(newfileContent))
                {
                    await File.WriteAllTextAsync(ymlFile.FullName, newfileContent).ConfigureAwait(false);
                }
            }

            // 4. Update directory.props if exists
            var directoryBuildProps = solutionFile.Directory!.EnumerateFiles("Directory.Build.props").FirstOrDefault();

            if (directoryBuildProps.IsNotNull())
            {
                // Read the directoryBuildProps file content and replace net8.0 with net9.0. Can you use a regex that the version can be any number
                var directoryBuildPropsContent = await File.ReadAllTextAsync(directoryBuildProps.FullName).ConfigureAwait(false);
                var newDirectoryBuildPropsContent = _netVersionReplaceRegex.Replace(directoryBuildPropsContent, $"net{versionParameters.Version}.0");
                newDirectoryBuildPropsContent = Regex.Replace(newDirectoryBuildPropsContent, @"\.NET \d+\.\d+", $".NET {versionParameters.Version}");
                await File.WriteAllTextAsync(directoryBuildProps.FullName, newDirectoryBuildPropsContent).ConfigureAwait(false);
            }

            // 5. Update all project files - because we have not already directory.props working in place
            var allProjectFiles = solutionFile.Directory!.EnumerateFiles("*.csproj", SearchOption.AllDirectories);

            foreach (var allProjectFile in allProjectFiles)
            {
                var projectFileContent = await File.ReadAllTextAsync(allProjectFile.FullName).ConfigureAwait(false);
                var newProjectFileContent = _netVersionReplaceRegex.Replace(projectFileContent, $"net{versionParameters.Version}.0");
                await File.WriteAllTextAsync(allProjectFile.FullName, newProjectFileContent).ConfigureAwait(false);
            }

            consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was successfully migrated to .Net version: {versionParameters.Version}");
        }
    }
}
