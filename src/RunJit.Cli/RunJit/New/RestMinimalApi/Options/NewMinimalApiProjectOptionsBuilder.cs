using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.New.RestMinimalApi.Options
{
    internal static class AddNewRestMinimalApiOptionsBuilderExtension
    {
        internal static void AddNewRestMinimalApiOptionsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<NewRestMinimalApiOptionsBuilder>();
        }
    }

    internal sealed class NewRestMinimalApiOptionsBuilder
    {
        public IEnumerable<Option> Build()
        {
            yield return GitRepos();
            yield return SolutionFile();
            yield return WorkingDirectory();
            yield return DomainModel();
            yield return Version();
            yield return QueryProperty();
        }

        public Option GitRepos()
        {
            return new Option(new[] { "--git-repos", "-gr" }, "The git repository urls. Sample: 'codecommit::eu-central-1://runjit-dbi' or multiple 'codecommit::eu-central-1://runjit-dbi;codecommit::eu-central-1://runjit-dbi' separated by ';'")
            {
                Required = false,
                Argument = new Argument<string>("gitRepos") { Description = "The git repository urls. Sample: 'codecommit::eu-central-1://runjit-dbi' or multiple 'codecommit::eu-central-1://runjit-dbi;codecommit::eu-central-1://runjit-dbi' separated by ';'" }
            };
        }

        public Option SolutionFile()
        {
            return new Option(new[] { "--solution", "-s" }, "The solution file '--solution D:\\My.sln' which should be updated")
            {
                Required = false,
                Argument = new Argument<FileInfo>("solution") { Description = "The solution file which should be updated" }
            };
        }

        public Option WorkingDirectory()
        {
            return new Option(new[] { "--working-directory", "-wd" }, "The working directory in which all operation should be executed. Only needed in combination with GitRepos")
            {
                Required = false,
                Argument = new Argument<string>("workingDirectory") { Description = "The working directory in which all operation should be executed" }
            };
        }
        
        public Option DomainModel()
        {
            return new Option(new[] { "--domain-model", "-dm" }, "Option to pass the domain model as c# class. Sample: public record User(string Name)")
            {
                Required = true,
                Argument = new Argument<string>("domainModel") { Description = "Option to pass the domain model as c# class. Sample: public record User(string Name)" }
            };
        }

        public Option Version()
        {
            return new Option(new[] { "--version", "-version" }, "Option to pass the api version as integer. 1 means V1")
            {
                Required = false,
                Argument = new Argument<int>("version") { Description = "Option to pass the api version as integer. 1 means V1" }
            };
        }

        public Option QueryProperty()
        {
            return new Option(new[] { "--filter-property", "-fp" }, "Property which is used to filter by by GetAll or DeleteAll operations. Sample 'Name'")
            {
                Required = true,
                Argument = new Argument<string>("filterProperty") { Description = "Property which is used to filter by by GetAll or DeleteAll operations. Sample 'Name'" }
            };
        }
    }
}
