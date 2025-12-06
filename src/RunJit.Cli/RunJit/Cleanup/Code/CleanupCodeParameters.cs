using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Cleanup.Code
{
    internal static class AddCleanupCodeParametersExtension
    {
        internal static void AddCleanupCodeParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<CleanupCodeParameters>();
        }
    }

    internal sealed record CleanupCodeParameters(string SolutionFile,
                                                 string GitRepos,
                                                 string WorkingDirectory,
                                                 string IgnorePackages);
}
