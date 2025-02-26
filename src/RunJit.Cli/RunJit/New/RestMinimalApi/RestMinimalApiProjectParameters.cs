using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.New.RestMinimalApi
{
    internal static class AddNewRestMinimalApiParametersExtension
    {
        internal static void AddNewRestMinimalApiParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<NewRestMinimalApiParameters>();
        }
    }

    internal sealed record NewRestMinimalApiParameters(FileInfo SolutionFile,
                                                       string GitRepos,
                                                       string WorkingDirectory,
                                                       string DomainModel,
                                                       int Version,
                                                       string QueryProperty);
}
