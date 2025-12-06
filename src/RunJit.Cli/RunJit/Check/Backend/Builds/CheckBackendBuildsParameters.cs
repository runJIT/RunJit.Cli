using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Check.Backend.Builds
{
    internal static class AddCheckBackendBuildsParametersExtension
    {
        internal static void AddCheckBackendBuildsParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<CheckBackendBuildsParameters>();
        }
    }

    internal sealed record CheckBackendBuildsParameters(string SolutionFile,
                                                        string GitRepos,
                                                        string WorkingDirectory,
                                                        string IgnorePackages);
}
