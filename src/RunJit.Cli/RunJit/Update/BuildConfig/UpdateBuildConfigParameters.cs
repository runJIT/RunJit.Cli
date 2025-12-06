using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Update.BuildConfig
{
    internal static class AddUpdateBuildConfigParametersExtension
    {
        internal static void AddUpdateBuildConfigParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UpdateBuildConfigParameters>();
        }
    }

    internal sealed record UpdateBuildConfigParameters(string SolutionFile,
                                                       string GitRepos,
                                                       string WorkingDirectory);
}
