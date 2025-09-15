using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Commands;
using RunJit.Cli.Test.Extensions;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit cleanup code")]
    [TestClass]
    public class CleanupCodeTest : GlobalSetup
    {
        private const string BasePath = "api/update";

        [TestMethod]
        public async Task Should_Cleanup_Code_In_Local_Solution()
        {
            // 1. Create new Web Api
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi("Simple.Project", WebApiFolder, BasePath)).ConfigureAwait(false);

            // 3. Test if generated results is buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);

            // 3. Update to .Net 8
            await Mediator.SendAsync(new CleanupCodeInSolution(solutionFile.FullName)).ConfigureAwait(false);
        }

        [TestMethod]

        //[DataRow("codecommit::eu-central-1://pulse-datamanagement")]
        //[DataRow("codecommit::eu-central-1://pulse-survey")]
        //[DataRow("codecommit::eu-central-1://pulse-core-service")]
        //[DataRow("codecommit::eu-central-1://pulse-actionmanagement")]
        //[DataRow("codecommit::eu-central-1://pulse-flow")]
        //[DataRow("codecommit::eu-central-1://pulse-documentmanagement")]
        //[DataRow("codecommit::eu-central-1://pulse-dbi")]
        //[DataRow("codecommit::eu-central-1://pulse-tableau")]
        [DataRow("codecommit::eu-central-1://pulse-powerbi")]

        //[DataRow("codecommit::eu-central-1://pulse-sustainability")]
        //[DataRow("codecommit::eu-central-1://pulse-estell")]
        //[DataRow("codecommit::eu-central-1://pulse-database")]
        public async Task Should_Cleanup_Code_From_GitRepo(string gitRepo)
        {
            await Mediator.SendAsync(new CleanupCodeForGitRepos(gitRepo, CodeCleanupFolder.FullName)).ConfigureAwait(false);
        }

        [Ignore]
        [TestMethod]
        [DataRow(@"D:\SoftwareOne\css-partners\SWO.CSS.OneSalesPartnerService.sln")]
        [DataRow(@"D:\AzureDevOps\SoftwareOne.Workshop.November.2023\RunJit\UserManagement\UserManagement.sln")]
        [DataRow(@"D:\AzureDevOps\AspNetCore.MinimalApi.Sdk\AspNetCore.MinimalApi.Sdk.sln")]
        public async Task Should_Update_All_CodeRules_Into_Specific_Local_Solution(string targetSolution)
        {
            // 1. Create new Web Api
            var solutionFile = new FileInfo(targetSolution);

            // 2. Test if target solution is build able
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);

            // 3. Update code rules
            await Mediator.SendAsync(new CleanupCodeInSolution(solutionFile.FullName)).ConfigureAwait(false);

            // 4. Test if integration was sucessful and buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);
        }

        [Ignore]
        [TestMethod]
        public Task Should_Update_Code_Rules_By_Cloning_First_A_Repo(string gitUrl)
        {
            // 1. Create new Web Api
            return Mediator.SendAsync(new CleanupCodeForGitRepos(gitUrl, CodeRuleFolder.FullName));
        }
    }

    internal sealed record CleanupCodeInSolution(string solution) : ICommand;

    internal sealed class CleanupCodeInSolutionHandler : ICommandHandler<CleanupCodeInSolution>
    {
        public async Task Handle(CleanupCodeInSolution request,
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

        private IEnumerable<string> CollectConsoleParameters(CleanupCodeInSolution parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "cleanup";
            yield return "code";
            yield return "--solution";
            yield return parameters.solution;
        }
    }

    internal sealed record CleanupCodeForGitRepos(string GitRepos,
                                                  string WorkingDirectory) : ICommand;

    internal sealed class CleanupCodeForGitReposHandler : ICommandHandler<CleanupCodeForGitRepos>
    {
        public async Task Handle(CleanupCodeForGitRepos request,
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

        private IEnumerable<string> CollectConsoleParameters(CleanupCodeForGitRepos parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "cleanup";
            yield return "code";
            yield return "--git-repos";
            yield return parameters.GitRepos;
            yield return "--working-directory";
            yield return parameters.WorkingDirectory;
        }
    }
}
