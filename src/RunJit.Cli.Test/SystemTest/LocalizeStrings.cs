using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit localize strings")]
    [TestClass]
    public class LocalizeStringsTest : GlobalSetup
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
        public async Task Localize_All_Strings(string gitUrl)
        {
            await Mediator.SendAsync(new LocalizeAllStrings(gitUrl, @"D:\LocalizeStrings")).ConfigureAwait(false);
        }
    }

    internal sealed record LocalizeAllStrings(string GitRepos,
                                              string WorkingDirectory) : ICommand;

    internal sealed class LocalizeAllStringsHandler : ICommandHandler<LocalizeAllStrings>
    {
        public async Task Handle(LocalizeAllStrings request,
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

        private IEnumerable<string> CollectConsoleParameters(LocalizeAllStrings parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "localize";
            yield return "strings";
            yield return "--git-repos";
            yield return parameters.GitRepos;
            yield return "--working-directory";
            yield return parameters.WorkingDirectory;
            yield return "--languages";
            yield return "en;de";
        }
    }
}
