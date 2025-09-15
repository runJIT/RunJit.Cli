using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Commands;
using RunJit.Cli.Test.Extensions;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit update nuget")]
    [TestClass]
    public class UpdateBackendNugetPackagesTest : GlobalSetup
    {
        private const string BasePath = "api/nuget-test";

        [TestMethod]
        public async Task Should_Update_All_Nuget_Packages_Target_Solution()
        {
            // 1. Create new Web Api
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi("RunJit.Update.Nuget", WebApiFolder, BasePath)).ConfigureAwait(false);

            // 2. Test if generated results is buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);

            // 3. Update nuget packages
            await Mediator.SendAsync(new UpdateBackendNugetPackagesForSolution(solutionFile.FullName)).ConfigureAwait(false);
        }

        [TestMethod]
        [DataRow("codecommit::eu-central-1://pulse-core")]
        [DataRow("codecommit::eu-central-1://pulse-flow")]
        [DataRow("codecommit::eu-central-1://pulse-datamanagement")]
        [DataRow("codecommit::eu-central-1://pulse-survey")]
        [DataRow("codecommit::eu-central-1://pulse-core-service")]
        [DataRow("codecommit::eu-central-1://pulse-actionmanagement")]
        [DataRow("codecommit::eu-central-1://pulse-documentmanagement")]
        [DataRow("codecommit::eu-central-1://pulse-dbi")]
        [DataRow("codecommit::eu-central-1://pulse-tableau")]
        [DataRow("codecommit::eu-central-1://pulse-powerbi")]
        [DataRow("codecommit::eu-central-1://pulse-sustainability")]
        [DataRow("codecommit::eu-central-1://pulse-estell")]
        [DataRow("codecommit::eu-central-1://pulse-database")]
        public async Task Check_Out_Update_Custom_Part_1(string gitUrl)
        {
            // 3. Update to .Net 8
            await Mediator.SendAsync(new UpdateBackendNugetPackagesForGitRepos(gitUrl, @"D:\NugetUpdate")).ConfigureAwait(false);
        }
    }

    internal sealed record UpdateBackendNugetPackagesForSolution(string solution) : ICommand;

    internal sealed class UpdateBackendNugetPackagesHandler : ICommandHandler<UpdateBackendNugetPackagesForSolution>
    {
        public async Task Handle(UpdateBackendNugetPackagesForSolution request,
                                 CancellationToken cancellationToken)
        {
            await using var sw = new StringWriter();
            Console.SetOut(sw);

            var strings = CollectConsoleParameters(request).ToArray();
            var consoleCall = strings.Flatten(" ");
            Console.WriteLine();
            Console.WriteLine(consoleCall);
            Debug.WriteLine(consoleCall);
            var exitCode = await Program.Main(strings).ConfigureAwait(false);
            var output = sw.ToString();

            Assert.AreEqual(0, exitCode, output);
        }

        private IEnumerable<string> CollectConsoleParameters(UpdateBackendNugetPackagesForSolution parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return "nuget";
            yield return "--solution";
            yield return parameters.solution;
        }
    }
    
    internal sealed record UpdateBackendNugetPackagesForGitRepos(string GitRepos, string WorkingDirectory) : ICommand;

    internal sealed class UpdateBackendNugetPackagesForGitReposHandler : ICommandHandler<UpdateBackendNugetPackagesForGitRepos>
    {
        public async Task Handle(UpdateBackendNugetPackagesForGitRepos request, CancellationToken cancellationToken)
        {
            await using var sw = new StringWriter();
            Console.SetOut(sw);

            var strings = CollectConsoleParameters(request).ToArray();
            var consoleCall = strings.Flatten(" ");
            Console.WriteLine();
            Console.WriteLine(consoleCall);
            Debug.WriteLine(consoleCall);
            var exitCode = await Program.Main(strings);
            var output = sw.ToString();

            Assert.AreEqual(0, exitCode, output);
        }

        private IEnumerable<string> CollectConsoleParameters(UpdateBackendNugetPackagesForGitRepos parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return "nuget";
            yield return "--git-repos";
            yield return parameters.GitRepos;
            yield return "--working-directory";
            yield return parameters.WorkingDirectory;
            yield return "--ignore-packages";
            yield return "EPPlus;MySql.Data;GitVersion.MsBuild";
        }
    }
}
