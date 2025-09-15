using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Commands;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit update net")]
    [TestClass]
    public class UpdateBackendNetVersionTest : GlobalSetup
    {
        private const string Resource = "User";

        private const string BasePath = "api/net-test";

        [TestMethod]
        public async Task UpdateNetVersion()
        {
            // 1. Create new Web Api
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi("RunJit.Update.Net", WebApiFolder, BasePath)).ConfigureAwait(false);

            // 2. Create Web-Api endpoints
            await Mediator.SendAsync(new CreateSimpleRestController(solutionFile, Resource, false)).ConfigureAwait(false);

            // 3. Update to .Net 9
            await Mediator.SendAsync(new UpdateNetVersion(solutionFile.FullName, 9)).ConfigureAwait(false);
        }

        [TestMethod]
        [DataRow("codecommit::eu-central-1://pulse-datamanagement")]
        [DataRow("codecommit::eu-central-1://pulse-survey")]
        [DataRow("codecommit::eu-central-1://pulse-core-service")]
        [DataRow("codecommit::eu-central-1://pulse-actionmanagement")]
        [DataRow("codecommit::eu-central-1://pulse-flow")]
        [DataRow("codecommit::eu-central-1://pulse-documentmanagement")]
        [DataRow("codecommit::eu-central-1://pulse-dbi")]
        [DataRow("codecommit::eu-central-1://pulse-tableau")]
        [DataRow("codecommit::eu-central-1://pulse-powerbi")]
        [DataRow("codecommit::eu-central-1://pulse-sustainability")]
        [DataRow("codecommit::eu-central-1://pulse-estell")]
        [DataRow("codecommit::eu-central-1://pulse-database")]
        [DataRow("codecommit::eu-central-1://pulse-common")]
        [DataRow("codecommit::eu-central-1://pulse-code-rules")]
        [DataRow("codecommit::eu-central-1://pulse-core")]
        public async Task Should_Update_Net_Version_For_On_Repo(string gitUrl)
        {
            await Mediator.SendAsync(new UpdateNetVersionForGitRepos(gitUrl, @"D:\UpdateNetVersion", 9)).ConfigureAwait(false);
        }
    }

    internal sealed record UpdateNetVersion(string solution,
                                            int version) : ICommand;

    internal sealed class UpdateBackendNetVersionHandler : ICommandHandler<UpdateNetVersion>
    {
        public async Task Handle(UpdateNetVersion request,
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

        private IEnumerable<string> CollectConsoleParameters(UpdateNetVersion parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return ".net";
            yield return parameters.solution;
            yield return "--version";
            yield return parameters.version.ToString();
        }
    }

    internal sealed record UpdateNetVersionForGitRepos(string GitRepos,
                                                       string WorkingDirectory,
                                                       int Version) : ICommand;

    internal sealed class UpdateNetVersionForGitReposHandler : ICommandHandler<UpdateNetVersionForGitRepos>
    {
        public async Task Handle(UpdateNetVersionForGitRepos request,
                                 CancellationToken cancellationToken)
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

        private IEnumerable<string> CollectConsoleParameters(UpdateNetVersionForGitRepos parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return ".net";
            yield return "--git-repos";
            yield return parameters.GitRepos;
            yield return "--working-directory";
            yield return parameters.WorkingDirectory;
            yield return "--version";
            yield return parameters.Version.ToInvariantString();
        }
    }
}
