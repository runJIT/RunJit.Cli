using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Update.Docu
{
    internal static class AddCheckBackendBuildsParametersExtension
    {
        internal static void AddCheckBackendBuildsParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UpdateDocuParameters>();
        }
    }

    internal sealed record UpdateDocuParameters(string SolutionFile,
                                                      string GitRepos,
                                                      string WorkingDirectory,
                                                      string IgnorePackages);
}
