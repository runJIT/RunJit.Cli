using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Fix.ProjectSettings
{
    internal static class AddFixProjectSettingsParametersExtension
    {
        internal static void AddFixProjectSettingsParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<FixProjectSettingsParameters>();
        }
    }

    internal sealed record FixProjectSettingsParameters(string SolutionFile,
                                                   string GitRepos,
                                                   string WorkingDirectory,
                                                   string IgnorePackages);
}
