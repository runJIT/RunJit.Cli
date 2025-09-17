using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit update net")]
    [TestClass]
    public class UpdateTargetPlatformTest : GlobalSetup
    {
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
        public async Task Should_Update_Target_Platform_Repo(string gitUrl)
        {
            await Mediator.SendAsync(new UpdateTargetPlatformForGitRepos(gitUrl, @"D:\UpdateTargetPlatform", "linux-arm64")).ConfigureAwait(false);
        }
    }

    internal sealed record UpdateTargetPlatform(string solution,
                                            int version) : ICommand;

    internal sealed class UpdateTargetPlatformHandler : ICommandHandler<UpdateTargetPlatform>
    {
        public async Task Handle(UpdateTargetPlatform request,
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

        private IEnumerable<string> CollectConsoleParameters(UpdateTargetPlatform parameters)
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

    internal sealed record UpdateTargetPlatformForGitRepos(string GitRepos,
                                                       string WorkingDirectory,
                                                       string Platform) : ICommand;

    internal sealed class UpdateTargetPlatformForGitReposHandler : ICommandHandler<UpdateTargetPlatformForGitRepos>
    {
        public async Task Handle(UpdateTargetPlatformForGitRepos request,
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

        private IEnumerable<string> CollectConsoleParameters(UpdateTargetPlatformForGitRepos parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return "platform";
            yield return "--git-repos";
            yield return parameters.GitRepos;
            yield return "--working-directory";
            yield return parameters.WorkingDirectory;
            yield return "--platform";
            yield return parameters.Platform;
        }
    }
}
