using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;

namespace RunJit.Cli.RunJit.Update.Nuget
{
    internal static class AddUpdateNugetExtension
    {
        internal static void AddUpdateNuget(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddUpdateNugetParameters();

            services.AddUpdateLocalSolutionFile();
            services.AddCloneReposAndUpdateAll();

            services.AddSingletonIfNotExists<IUpdateNuget, UpdateNuget>();
        }
    }

    internal interface IUpdateNuget
    {
        Task HandleAsync(UpdateNugetParameters parameters);
    }

    internal sealed class UpdateNuget(IEnumerable<IUpdateNugetStrategy> updateNugetStrategies) : IUpdateNuget
    {
        public Task HandleAsync(UpdateNugetParameters parameters)
        {
            var updateNugetStrategy = updateNugetStrategies.Where(x => x.CanHandle(parameters)).ToImmutableList();

            if (updateNugetStrategy.Count < 1)
            {
                throw new RunJitException($"Could not find a strategy a update nuget strategy for parameters: {parameters}");
            }

            if (updateNugetStrategy.Count > 1)
            {
                throw new RunJitException($"Found more than one strategy a update nuget strategy for parameters: {parameters}");
            }

            return updateNugetStrategy[0].HandleAsync(parameters);
        }
    }

    internal interface IUpdateNugetStrategy
    {
        bool CanHandle(UpdateNugetParameters parameters);

        Task HandleAsync(UpdateNugetParameters parameters);
    }

    public class Framework
    {
        // public string Framework { get; init; } = string.Empty;
        public ImmutableList<TopLevelPackage> TopLevelPackages { get; init; } = ImmutableList<TopLevelPackage>.Empty;
    }

    public class Project
    {
        public string Path { get; init; } = string.Empty;

        public ImmutableList<Framework> Frameworks { get; init; } = ImmutableList<Framework>.Empty;
    }

    public class OutdatedNugetResponse
    {
        public int Version { get; init; }

        public string Parameters { get; init; } = string.Empty;

        public ImmutableList<string> Sources { get; init; } = ImmutableList<string>.Empty;

        public ImmutableList<Project> Projects { get; init; } = ImmutableList<Project>.Empty;
    }

    public class TopLevelPackage
    {
        public string Id { get; init; } = string.Empty;

        public string RequestedVersion { get; init; } = string.Empty;

        public string ResolvedVersion { get; init; } = string.Empty;

        public string LatestVersion { get; init; } = string.Empty;
    }
}
