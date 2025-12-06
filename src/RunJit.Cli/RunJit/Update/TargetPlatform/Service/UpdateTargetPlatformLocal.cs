using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Update.TargetPlatform
{
    internal static class AddUpdateTargetPlatformLocalExtension
    {
        internal static void AddUpdateTargetPlatformLocal(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddUpdateTargetPlatformParameters();
            services.AddFindSolutionFile();

            services.AddSingletonIfNotExists<UpdateTargetPlatformLocal>();
        }
    }

    internal sealed class UpdateTargetPlatformLocal(ConsoleService consoleService,
                                                    FindSolutionFile findSolutionFile)
    {
        public async Task HandleAsync(UpdateTargetPlatformParameters parameters)
        {
            // 1. Check if solution file is the file or directory
            //    if it is null or whitespace we check current directory
            var solutionFile = findSolutionFile.Find(parameters.SolutionFile);

            // 2. We need a global json
            var globalJsonFilePath = Path.Combine(solutionFile.Directory!.FullName, "global.json");
            var globalJsonFileInfo = new FileInfo(globalJsonFilePath);

            if (globalJsonFileInfo.NotExists())
            {
                var template = EmbeddedFile.GetFileContentFrom("RunJit.Update.TargetPlatform.Templates.global.json");
                await File.WriteAllTextAsync(globalJsonFileInfo.FullName, template);
            }

            // 3. We have to modify docker file as well
            // # Base runtime image
            // FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:9.0-alpine3.21 AS base
            // WORKDIR /app
            // EXPOSE 80

            // # Build image
            // FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.21 AS build
            // WORKDIR /src
            var dockerFiles = solutionFile.Directory!.EnumerateFiles("Dockerfile");

            foreach (var dockerFile in dockerFiles)
            {
                var lines = await File.ReadAllLinesAsync(dockerFile.FullName);

                for (int i = 0; i < lines.Length; i++)
                {
                    var currentLine = lines[i];

                    if (currentLine.EndsWith("AS base", StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = "FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:9.0-alpine3.21 AS base";
                    }

                    if (currentLine.EndsWith("AS build"))
                    {
                        lines[i] = "FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.21 AS build";
                    }

                    if (currentLine.Contains("dotnet build ") && currentLine.DoesNotContain("--runtime"))
                    {
                        lines[i] = $"{lines[i]} --runtime {parameters.Platform}";
                    }

                    if (currentLine.Contains("dotnet publish ") && currentLine.DoesNotContain("--runtime"))
                    {
                        lines[i] = $"{lines[i]} --runtime {parameters.Platform}";
                    }
                }

                File.WriteAllLines(dockerFile.FullName, lines);
            }

            consoleService.WriteSuccess($"Solution: {solutionFile.FullName} migrated to: {parameters.Platform}");
        }
    }
}
