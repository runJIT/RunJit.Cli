using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Commands;
using RunJit.Cli.Test.Extensions;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit update coderules")]
    [TestClass]
    public class UpdateCodeRulesTest : GlobalSetup
    {
        private const string BasePath = "api/cleanup";

        [TestMethod]
        public async Task Should_Update_All_CodeRules_For_Target_Solution()
        {
            // 1. Create new Web Api
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi("Simple.Project", WebApiFolder, BasePath)).ConfigureAwait(false);

            // 3. Test if generated results is buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);

            // 3. Update to .Net 8
            await Mediator.SendAsync(new UpdateCodeRulesForSolution(solutionFile.FullName)).ConfigureAwait(false);
        }

        // [Ignore]
        [TestMethod]
        [DataRow(@"D:\Siemens\pulse-database\Pulse.Database.sln")]
        [DataRow(@"D:\SoftwareOne\css-partners\SWO.CSS.OneSalesPartnerService.sln")]
        [DataRow(@"D:\AzureDevOps\SoftwareOne.Workshop.November.2023\RunJit\UserManagement\UserManagement.sln")]
        [DataRow(@"D:\AzureDevOps\AspNetCore.MinimalApi.Sdk\AspNetCore.MinimalApi.Sdk.sln")]
        [DataRow(@"D:\Siemens\siemensgpt-backend\SiemensGPT.sln")]
        [DataRow(@"D:\Siemens\siemens-aspnet-sdk\Siemens.AspNet.Sdk.sln")]
        public async Task Should_Update_All_CodeRules_Into_Specific_Local_Solution(string targetSolution)
        {
            // 1. Create new Web Api
            var solutionFile = new FileInfo(targetSolution);

            // 2. Test if target solution is build able
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);

            // 3. Update code rules
            await Mediator.SendAsync(new UpdateCodeRulesForSolution(solutionFile.FullName)).ConfigureAwait(false);

            // 4. Test if integration was sucessful and buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);
        }

        [Ignore]
        [TestMethod]
        [DataRow(@"https://softwareone-ca@dev.azure.com/softwareone-ca/Sales%20and%20Marketing/_git/css-partners")]
        public Task Should_Update_Code_Rules_By_Cloning_First_A_Repo(string gitUrl)
        {
            // 1. Create new Web Api
            return Mediator.SendAsync(new UpdateCodeRulesPackagesForGitRepos(gitUrl, CodeRuleFolder.FullName));
        }
    }

    internal sealed record UpdateCodeRulesForSolution(string solution) : ICommand;

    internal sealed class UpdateCodeRulesForSolutionHandler : ICommandHandler<UpdateCodeRulesForSolution>
    {
        public async Task Handle(UpdateCodeRulesForSolution request,
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

        private IEnumerable<string> CollectConsoleParameters(UpdateCodeRulesForSolution parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return "coderules";
            yield return "--solution";
            yield return parameters.solution;
        }
    }

    internal sealed record UpdateCodeRulesPackagesForGitRepos(string GitRepos,
                                                              string WorkingDirectory) : ICommand;

    internal sealed class UpdateCodeRulesPackagesForGitReposHandler : ICommandHandler<UpdateCodeRulesPackagesForGitRepos>
    {
        public async Task Handle(UpdateCodeRulesPackagesForGitRepos request,
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

        private IEnumerable<string> CollectConsoleParameters(UpdateCodeRulesPackagesForGitRepos parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return "coderules";
            yield return "--git-repos";
            yield return parameters.GitRepos;
            yield return "--working-directory";
            yield return parameters.WorkingDirectory;
            yield return "--branch";
            yield return "develop";
        }
    }
}
