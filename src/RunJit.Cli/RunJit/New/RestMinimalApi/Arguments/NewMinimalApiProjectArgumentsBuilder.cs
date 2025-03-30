using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Check.Backend.Builds
{
    internal static class AddNewMinimalApiProjectArgumentsBuilderExtension
    {
        internal static void AddNewMinimalApiProjectArgumentsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<NewMinimalApiProjectArgumentsBuilder>();
        }
    }

    internal sealed class NewMinimalApiProjectArgumentsBuilder
    {
        public IEnumerable<System.CommandLine.Argument> Build()
        {
            yield return BuildSolutionOrGitRepos();
        }

        public System.CommandLine.Argument BuildSolutionOrGitRepos()
        {
            return new Argument<string>
                   {
                       Name = "solutionFileOrGitRepos",
                       Description = @"Please pass your absolute path to your solution file (sample: D:\Siemens\siemens-aspnet-errorhandler\Siemens.AspNet.ErrorHandler.sln) or git repository urls (sample: 'https://github.siemens.cloud/sdc/siemens-aspnet-errorhandler.git' or multiple 'codecommit::eu-central-1://pulse-datamanagement;https://github.siemens.cloud/sdc/siemens-aspnet-errorhandler.git' separated by ';'",
            };
        }
    }
}
