﻿using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Net;

namespace RunJit.Cli.RunJit.Update.Nuget
{
    public static class AddUpdateNugetPackageServiceExtension
    {
        public static void AddUpdateNugetPackageService(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IUpdateNugetPackageService, UpdateNugetPackageService>();
        }
    }

    internal interface IUpdateNugetPackageService
    {
        Task UpdateNugetPackageAsync(OutdatedNugetResponse outdatedNugetResponse, IImmutableList<string> packagesToIgnore);
    }

    internal class UpdateNugetPackageService(IConsoleService consoleService,
                                             IDotNet dotnet) : IUpdateNugetPackageService
    {
        public async Task UpdateNugetPackageAsync(OutdatedNugetResponse outdatedNugetResponse, IImmutableList<string> packagesToIgnore)
        {
            // for each outdated package we need to update the package
            foreach (var project in outdatedNugetResponse.Projects)
            {
                foreach (var framework in project.Frameworks)
                {
                    foreach (var package in framework.TopLevelPackages)
                    {
                        if (packagesToIgnore.Any(p => p.ToUpperInvariant() == package.Id.ToUpperInvariant()))
                        {
                            consoleService.WriteSuccess($"Skip package: {package.Id} because it was on the ignore list");
                            continue;
                        }

                        if (package.ResolvedVersion == package.LatestVersion)
                        {
                            continue;
                        }

                        consoleService.WriteInfo($"Update package: {package.Id} from version: {package.ResolvedVersion} to version: {package.LatestVersion} in project: {project.Path}");
                        await dotnet.AddNugetPackageAsync(project.Path, package.Id, package.LatestVersion).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
