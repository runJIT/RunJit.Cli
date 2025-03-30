using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Update.Docu
{
    internal static class AddUpdateDocuBuildsOptionsBuilderExtension
    {
        internal static void AddUpdateDocuBuildsOptionsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UpdateDocuBuildsOptionsBuilder>();
        }
    }

    internal sealed class UpdateDocuBuildsOptionsBuilder
    {
        public IEnumerable<Option> Build()
        {
            yield return GitRepos();
            yield return SolutionFile();
            yield return WorkingDirectory();
            yield return DocuFile();
        }

        public Option GitRepos()
        {
            return new Option(new[] { "--git-repos", "-gr" }, "The git repository urls. Sample: 'codecommit::eu-central-1://runjit-dbi' or multiple 'codecommit::eu-central-1://runjit-dbi;codecommit::eu-central-1://runjit-dbi' separated by ';'")
            {
                Required = false,
                Argument = new Argument<string>("gtiRepos") { Description = "The git repository urls. Sample: 'codecommit::eu-central-1://runjit-dbi' or multiple 'codecommit::eu-central-1://runjit-dbi;codecommit::eu-central-1://runjit-dbi' separated by ';'" }
            };
        }

        public Option SolutionFile()
        {
            return new Option(new[] { "--solution", "-s" }, "The solution file which should be updated")
            {
                Required = false,
                Argument = new Argument<string>("solution") { Description = "The solution file which should be updated" }
            };
        }

        public Option WorkingDirectory()
        {
            return new Option(new[] { "--working-directory", "-wd" }, "The working directory in which all operation should be executed")
            {
                Required = false,
                Argument = new Argument<string>("solutionFile") { Description = "The working directory in which all operation should be executed" }
            };
        }

        public Option DocuFile()
        {
            return new Option(new[] { "--globalJsonFile", "-gjf" }, "The json as native string or the path of the global json file which should be used")
            {
                Required = false,
                Argument = new Argument<string>("globalJsonFile") { Description = "The json as native string or the path of the global json file which should be used" }
            };
        }
    }
}
