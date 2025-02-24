using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Fix.ProjectSettings
{
    internal static class AddFixProjectSettingsOptionsBuilderExtension
    {
        internal static void AddFixProjectSettingsOptionsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IFixProjectSettingsOptionsBuilder, FixProjectSettingsOptionsBuilder>();
        }
    }

    internal interface IFixProjectSettingsOptionsBuilder
    {
        IEnumerable<Option> Build();
    }

    internal sealed class FixProjectSettingsOptionsBuilder : IFixProjectSettingsOptionsBuilder
    {
        public IEnumerable<Option> Build()
        {
            yield return GitRepos();
            yield return SolutionFile();
            yield return WorkingDirectory();
            yield return IgnorePackage();
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

        public Option IgnorePackage()
        {
            return new Option(new[] { "--ignore-packages", "-ip" }, "Option to give the package name with it which should not be updated. Sample. EPP or commaseparated multiple. EPP;MySql")
                   {
                       Required = false,
                       Argument = new Argument<string>("ignorePackages") { Description = "Option to give the package name with it which should not be updated. Sample. EPP or commaseparated multiple. EPP;MySql" }
                   };
        }
    }
}
