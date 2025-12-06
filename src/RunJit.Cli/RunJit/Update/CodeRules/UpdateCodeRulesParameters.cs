using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Update.CodeRules
{
    internal static class AddUpdateCodeRulesParametersExtension
    {
        internal static void AddUpdateCodeRulesParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UpdateCodeRulesParameters>();
        }
    }

    internal sealed record UpdateCodeRulesParameters(string SolutionFile,
                                                     string GitRepos,
                                                     string WorkingDirectory,
                                                     string IgnorePackages,
                                                     string Branch);
}
