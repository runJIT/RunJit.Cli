using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Localize.Strings
{
    internal static class AddLocalizeStringsParametersExtension
    {
        internal static void AddLocalizeStringsParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<LocalizeStringsParameters>();
        }
    }

    internal sealed record LocalizeStringsParameters(string SolutionFile,
                                                     string GitRepos,
                                                     string WorkingDirectory,
                                                     ImmutableList<string> Languages);
}
