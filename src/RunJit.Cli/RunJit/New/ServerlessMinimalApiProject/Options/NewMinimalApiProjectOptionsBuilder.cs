using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.New.MinimalApiProject
{
    internal static class AddNewMinimalApiProjectOptionsBuilderExtension
    {
        internal static void AddNewMinimalApiProjectOptionsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<NewMinimalApiProjectOptionsBuilder>();
        }
    }

    internal sealed class NewMinimalApiProjectOptionsBuilder
    {
        public IEnumerable<Option> Build()
        {
            yield return BuildUseVisualStudioOption();
            yield return Solution();
            yield return BuildMsBuild();
            yield return ProjectName();
            yield return TargetDirectory();
            yield return BasePath();
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

        private Option BasePath()
        {
            return new Option(new[] { "--base-path", "-bp" }, "The base path for the new minimal api. Sample: \"api/core\"")
                   {
                       Required = true,
                       Argument = new Argument<string>("basePath") { Description = "The base path for the new minimal api. Sample: \"api/core\"" }
                   };
        }

        private Option TargetDirectory()
        {
            return new Option(new[] { "--target-directory", "-td" }, @"The target directory where the new minimal api project will be created. Sample: D:\Projects\DotNetToolGen")
                   {
                       Required = false,
                       Argument = new Argument<DirectoryInfo>("targetDirectory") { Description = @"The target directory where the new minimal api project will be created. Sample: D:\Projects\DotNetToolGen" }
                   };
        }

        private Option BuildUseVisualStudioOption()
        {
            return new Option(new[] { "--use-visualstudio", "-uv" }, "Opens visual studio after generating the new dotnet tool. Only for Windows user !") { Required = false };
        }

        private Option BuildMsBuild()
        {
            return new Option(new[] { "--build", "-b" }, "Builds the target solution before creating the client") { Required = false };
        }

        private Option Solution()
        {
            return new Option(new[] { "--solution", "-s" }, @"File path to your backend solution where your api is implemented. Sample: D:\Projects\DotNetToolGen\DotNetToolGen.sln")
                   {
                       Required = false,
                       Argument = new Argument<FileInfo>("solution") { Description = @"File path to your backend solution where your api is implemented. Sample: D:\Projects\DotNetToolGen\DotNetToolGen.sln" }
                   };
        }

        private Option ProjectName()
        {
            return new Option(new[] { "--project-name", "-pn" }, "The name of your new minimal api backend project (i.e. \"Siemens.Core\", \"Siemens.DataManagement\", ...)")
                   {
                       Required = true,
                       Argument = new Argument<string>("projectName") { Description = "The name of your new minimal api backend project (i.e. \"Siemens.Core\", \"Siemens.DataManagement\", ...)" }
                   };
        }
    }
}
