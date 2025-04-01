using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Commands;
using RunJit.Cli.Test.Extensions;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit update docu")]
    [TestClass]
    public class UpdateDocuTests : GlobalSetup
    {
        private const string BasePath = "api/cleanup";

        [TestMethod]
        public async Task Should_Update_All_Docu_For_Target_Solution()
        {
            // 1. Create new Web Api
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi("Simple.Project", WebApiFolder, BasePath)).ConfigureAwait(false);

            // 3. Test if generated results is buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);

            // 3. Update to .Net 8
            await Mediator.SendAsync(new UpdateDocuForSolution(solutionFile.FullName)).ConfigureAwait(false);
        }

        // [Ignore]
        [DataTestMethod]
        [DataRow(@"D:\Siemens\pulse-fieldingtool\Pulse.FieldingTool.sln")]
        public async Task Should_Update_All_Docu_Into_Specific_Local_Solution(string targetSolution)
        {
            // 1. Create new Web Api
            var solutionFile = new FileInfo(targetSolution);

            //// 2. Test if target solution is build able
            //await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);

            // 3. Update code rules
            await Mediator.SendAsync(new UpdateDocuForSolution(solutionFile.FullName)).ConfigureAwait(false);

            // 4. Test if integration was sucessful and buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);
        }

        [Ignore]
        [DataTestMethod]
        [DataRow(@"https://softwareone-ca@dev.azure.com/softwareone-ca/Sales%20and%20Marketing/_git/css-partners")]
        public Task Should_Update_Code_Rules_By_Cloning_First_A_Repo(string gitUrl)
        {
            // 1. Create new Web Api
            return Mediator.SendAsync(new UpdateDocuForGitRepos(gitUrl, CodeRuleFolder.FullName));
        }
    }

    internal sealed record UpdateDocuForSolution(string solution) : ICommand;

    internal sealed class UpdateDocuForSolutionHandler : ICommandHandler<UpdateDocuForSolution>
    {
        public async Task Handle(UpdateDocuForSolution request,
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

        private IEnumerable<string> CollectConsoleParameters(UpdateDocuForSolution parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return "docu";
            yield return "--solution";
            yield return parameters.solution;
        }
    }

    internal sealed record UpdateDocuForGitRepos(string GitRepos,
                                                              string WorkingDirectory) : ICommand;

    internal sealed class UpdateDocuForGitReposHandler : ICommandHandler<UpdateDocuForGitRepos>
    {
        public async Task Handle(UpdateDocuForGitRepos request,
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

        private IEnumerable<string> CollectConsoleParameters(UpdateDocuForGitRepos parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return "docu";
            yield return "--git-repos";
            yield return parameters.GitRepos;
            yield return "--working-directory";
            yield return parameters.WorkingDirectory;
        }
    }
}
