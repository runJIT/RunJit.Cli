using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Update.GlobalJson
{
    internal static class AddCheckBackendBuildsParametersExtension
    {
        internal static void AddCheckBackendBuildsParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UpdateGlobalJsonParameters>();
        }
    }

    internal sealed record UpdateGlobalJsonParameters(string SolutionFile,
                                                      string GitRepos,
                                                      string WorkingDirectory,
                                                      string IgnorePackages);
}
