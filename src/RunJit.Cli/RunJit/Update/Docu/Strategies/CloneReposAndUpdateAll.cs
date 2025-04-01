using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;
using RunJit.Cli.Update.GlobalJson;

namespace RunJit.Cli.Update.Docu
{
    internal static class AddCloneReposAndUpdateAllExtension
    {
        internal static void AddCloneReposAndUpdateAll(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddGitService();
            services.AddDotNet();
            services.AddAwsCodeCommit();
            services.AddFindSolutionFile();

            services.AddSingletonIfNotExists<IUpdateDocuServiceStrategy, CloneReposAndUpdateAll>();
        }
    }

    internal sealed class CloneReposAndUpdateAll() : IUpdateDocuServiceStrategy
    {
        public bool CanHandle(UpdateDocuParameters parameters)
        {
            return parameters.SolutionFile.IsNullOrWhiteSpace();
        }

        public Task HandleAsync(UpdateDocuParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
