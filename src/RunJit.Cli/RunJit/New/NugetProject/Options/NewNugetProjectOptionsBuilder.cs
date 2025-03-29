using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.New.NugetProject
{
    internal static class AddNewNugetProjectOptionsBuilderExtension
    {
        internal static void AddNewNugetProjectOptionsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<NewNugetProjectOptionsBuilder>();
        }
    }

    internal sealed class NewNugetProjectOptionsBuilder
    {
        public IEnumerable<Option> Build()
        {
            yield return StartIdeAfterGeneration();
            yield return ProjectName();
            yield return TargetDirectory();
            yield return DotNetVersion();
        }

        private Option DotNetVersion()
        {
            return new Option(new[] { "--target-framework", "-tf" }, "The .Net version for the new minimal api. Sample: 9")
            {
                Required = false,
                Argument = new Argument<int>("targetFramework") { Description = "The .Net version for the new minimal api. Sample: 9" }
            };
        }

        private Option TargetDirectory()
        {
            return new Option(new[] { "--target-directory", "-td" }, @"The target directory where the new minimal api project will be created. Sample: D:\Projects\DotNetToolGen")
            {
                Required = false,
                Argument = new Argument<DirectoryInfo>("targetDirectory")
                {
                    Description = @"The target directory where the new minimal api project will be created. Sample: D:\Projects\DotNetToolGen"
                }
            };
        }

        private Option StartIdeAfterGeneration()
        {
            return new Option(new[] { "--start-ide", "-si" }, "Start your current installed IDE. Search route VisualStudio, Rider, VsCode") { Required = false };
        }

        private Option ProjectName()
        {
            return new Option(new[] { "--project-name", "-pn" }, "The name of your new minimal api backend project (i.e. \"Siemens.Core\", \"Siemens.DataManagement\", ...)")
            {
                Required = true,
                Argument = new Argument<string>("projectName")
                {
                    Description = "The name of your new minimal api backend project (i.e. \"Siemens.Core\", \"Siemens.DataManagement\", ...)"
                }
            };
        }
    }
}
