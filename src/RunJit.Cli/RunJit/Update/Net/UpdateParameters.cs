using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Update
{
    internal static class AddDotNetParametersExtension
    {
        internal static void AddUpdateDotNetVersionParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UpdateDotNetVersionParameters>();
        }
    }

    internal sealed record UpdateDotNetVersionParameters(string SolutionFile,
                                                         string GitRepos,
                                                         string WorkingDirectory,
                                                         int Version);
}
