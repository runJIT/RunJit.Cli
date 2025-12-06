using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Update.Nuget
{
    internal static class AddUpdateNugetParametersExtension
    {
        internal static void AddUpdateNugetParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UpdateNugetParameters>();
        }
    }

    internal sealed record UpdateNugetParameters(string SolutionFile,
                                                 string GitRepos,
                                                 string WorkingDirectory,
                                                 string IgnorePackages);
}
