using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Commands;
using RunJit.Cli.Test.Extensions;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit fix embeddedresources")]
    [TestClass]
    public class FixEmbeddedResourceTest : GlobalSetup
    {
        private const string Resource = "User";

        private const string BasePath = "api/client-gen";

        [TestMethod]
        public async Task Fix_All_EmbeddedResources()
        {
            // 1. Create new Web Api
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi("RunJit.Fix.EmbeddedResources", WebApiFolder, BasePath)).ConfigureAwait(false);

            // 2. Create Web-Api endpoints
            await Mediator.SendAsync(new CreateSimpleRestController(solutionFile, Resource, false)).ConfigureAwait(false);

            // 3. Generate client for endpoint
            await Mediator.SendAsync(new FixEmbeddedResourceLocally(solutionFile.FullName)).ConfigureAwait(false);

            //// 4. Test if generated results is buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);
        }

        [Ignore("Dev only")]
        [TestMethod]
        [DataRow("codecommit::eu-central-1://pulse-datamanagement")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-survey")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-core-service")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-actionmanagement")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-flow")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-documentmanagement")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-dbi")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-tableau")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-powerbi")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-sustainability")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-estell")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-database")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-common")] // Merged
        [DataRow("codecommit::eu-central-1://pulse-code-rules")]
        [DataRow("codecommit::eu-central-1://pulse-core")] // Merged
        public async Task Fix_All_Embedded_Resources_In(string gitUrl)
        {
            // 3. Update to .Net 8
            await Mediator.SendAsync(new FixEmbeddedResource(gitUrl, @"D:\EmbeddedResource")).ConfigureAwait(false);
        }

        [TestMethod]
        [DataRow(@"D:\GitHub\RunJit.Api\RunJit.Api.sln")]
        [DataRow(@"D:\Siemens\pulse-common\Pulse.Common.sln")]
        [DataRow(@"D:\Siemens\pulse-survey\PulseSurvey.sln")]
        [DataRow(@"D:\Siemens\pulse-core\PulseCore.sln")]
        [DataRow(@"D:\AzureDevOps\AspNetCore.MinimalApi.Sdk\AspNetCore.MinimalApi.Sdk.sln")]
        [DataRow(@"D:\Siemens\pulse-sustainability\Pulse.Sustainability.sln")]
        [DataRow("/Users/z003m9sc/Documents/RiderProjects/SiemensGPT/siemensgpt-backend/SiemensGPT.sln")]
        [DataRow("/Users/z003m9sc/Documents/RiderProjects/PulseCloud/pulse-nexus/Pulse.Nexus.sln")]
        [DataRow(@"D:\SoftwareOne\css-opportunity-api\SWO.CSS.Opportunity.sln")]
        [DataRow(@"D:\SoftwareOne\css-lead-api\SWO.CSS.LeadApi.sln")]
        public Task Fix_Embedded_Resources_In(string solutionPath)
        {
            return Mediator.SendAsync(new FixEmbeddedResourceLocally(solutionPath));
        }
    }

    internal sealed record FixEmbeddedResource(string GitRepos,
                                               string WorkingDirectory) : ICommand;

    internal sealed class FixEmbeddedResourceHandler : ICommandHandler<FixEmbeddedResource>
    {
        public async Task Handle(FixEmbeddedResource request,
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

        private IEnumerable<string> CollectConsoleParameters(FixEmbeddedResource parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "fix";
            yield return "embeddedresource";
            yield return "--git-repos";
            yield return parameters.GitRepos;
            yield return "--working-directory";
            yield return parameters.WorkingDirectory;
        }
    }

    internal sealed record FixEmbeddedResourceLocally(string Solution) : ICommand;

    internal sealed class FixEmbeddedResourceLocallyHandler : ICommandHandler<FixEmbeddedResourceLocally>
    {
        public async Task Handle(FixEmbeddedResourceLocally request,
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

        private IEnumerable<string> CollectConsoleParameters(FixEmbeddedResourceLocally parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "fix";
            yield return "embeddedresource";
            yield return "--solution";
            yield return parameters.Solution;
        }
    }
}
