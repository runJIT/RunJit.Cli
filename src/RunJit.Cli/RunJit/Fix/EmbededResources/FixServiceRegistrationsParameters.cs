using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Fix.EmbededResources
{
    internal static class AddFixEmbeddedResourcesParametersExtension
    {
        internal static void AddFixEmbeddedResourcesParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<FixEmbeddedResourcesParameters>();
        }
    }

    internal sealed record FixEmbeddedResourcesParameters(string SolutionFile,
                                                          string GitRepos,
                                                          string WorkingDirectory,
                                                          string IgnorePackages);
}
