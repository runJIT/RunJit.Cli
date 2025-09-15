using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Commands;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit update buildconfig")]
    [TestClass]
    public class UpdateBuildConfigTest : GlobalSetup
    {
        private const string Resource = "User";

        private const string BasePath = "api/net-test";

        [TestMethod]
        public async Task Should_Update_Build_Configurations_For_Target_Solution()
        {
            // 1. Create new Web Api
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi("RunJit.Update.BuildConfig", WebApiFolder, BasePath)).ConfigureAwait(false);

            // 2. Create Web-Api endpoints
            await Mediator.SendAsync(new CreateSimpleRestController(solutionFile, Resource, false)).ConfigureAwait(false);

            // 3. Update build configurations like Directory.Build.props
            await Mediator.SendAsync(new UpdateBuildConfigLocally(solutionFile.FullName, 8)).ConfigureAwait(false);
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
        public async Task Update_Build_Configurations_In(string gitUrl)
        {
            // 3. Update to .Net 8
            await Mediator.SendAsync(new UpdateBuildConfig(gitUrl, @"D:\UpdateBuildConfig")).ConfigureAwait(false);
        }
    }

    internal sealed record UpdateBuildConfigLocally(string solution,
                                                    int version) : ICommand;

    internal sealed class UpdateBuildConfigLocallyHandler : ICommandHandler<UpdateBuildConfigLocally>
    {
        public async Task Handle(UpdateBuildConfigLocally request,
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

        private IEnumerable<string> CollectConsoleParameters(UpdateBuildConfigLocally parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return "buildconfig";
            yield return parameters.solution;
        }
    }

    internal sealed record UpdateBuildConfig(string GitRepos,
                                             string WorkingDirectory) : ICommand;

    internal sealed class UpdateBuildConfigHandler : ICommandHandler<UpdateBuildConfig>
    {
        public async Task Handle(UpdateBuildConfig request,
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

        private IEnumerable<string> CollectConsoleParameters(UpdateBuildConfig parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return "buildconfig";
            yield return "--git-repos";
            yield return parameters.GitRepos;
            yield return "--working-directory";
            yield return parameters.WorkingDirectory;
        }
    }
}
