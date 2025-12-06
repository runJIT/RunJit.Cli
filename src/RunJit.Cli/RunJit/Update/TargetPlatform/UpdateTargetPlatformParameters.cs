using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Update.TargetPlatform
{
    internal static class AddDotNetParametersExtension
    {
        internal static void AddUpdateTargetPlatformParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UpdateTargetPlatformParameters>();
        }
    }

    internal sealed record UpdateTargetPlatformParameters(string SolutionFile,
                                                          string GitRepos,
                                                          string WorkingDirectory,
                                                          string Platform);
}
