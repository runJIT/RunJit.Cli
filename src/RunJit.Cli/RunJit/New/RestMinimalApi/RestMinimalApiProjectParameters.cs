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

    internal sealed record NewRestMinimalApiParameters(string SolutionFilesOrGitRepos,
                                                       string BasePath,
                                                       string WorkingDirectory,
                                                       string DbEntityModel,
                                                       int Version,
                                                       string QueryProperty,
                                                       string DomainName);
}
