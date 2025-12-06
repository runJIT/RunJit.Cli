using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Commands;
using RunJit.Cli.Test.Extensions;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit new lambda")]
    [TestClass]
    public class NewLambdaTest : GlobalSetup
    {
        private const string BasePath = "api/lambdas";

        [TestMethod]
        [DataRow("Pulse.Lambdas.Gpt1", "core", "CallGpt",
                    "analytics-gpt-chat")]
        [DataRow("Pulse.Lambdas.Gpt2", "Core", "CallGpt",
                    "analytics-gpt-chat")]
        [DataRow("Pulse.Lambdas.Gpt3", "Core", "CallGpt1",
                    "analytics-gpt-chat")]
        [DataRow("Pulse.Lambdas.Gpt4", "Core", "CallGpt1",
                    "analytics-gpt-chat1")]
        public async Task Should_Create_A_New_Lambda_And_Integrate_It_Into_Target_Solution(string projectName,
                                                                                           string moduleName,
                                                                                           string functionName,
                                                                                           string lambdaName)
        {
            // 1. Create new Solution
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi(projectName, WebApiFolder, BasePath)).ConfigureAwait(false);

            // 2. Generate lambda
            await Mediator.SendAsync(new GenerateLambda(solutionFile, moduleName, functionName,
                                                        lambdaName)).ConfigureAwait(false);

            // 3. Test if generated results is buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);

            // 4. Run tests if they exists
            await DotNetTool.AssertRunAsync("dotnet", $"test {solutionFile.FullName}").ConfigureAwait(false);
        }

        [TestMethod]
        [DataRow("Pulse.Lambdas.Gpt10", "core!", "CallGpt",
                    "analytics-gpt-chat", "ModuleName should contain no special characters other than '-'. \nExample: 'core'")]
        [DataRow("Pulse.Lambdas.Gpt11", "core", "1CallGpt",
                    "analytics-gpt-chat", "FunctionName should be alphanumeric and not begin with a number. \nExample: 'CallGpt'")]
        [DataRow("Pulse.Lambdas.Gpt12", "core", "CallGpt!",
                    "analytics-gpt-chat", "FunctionName should be alphanumeric and not begin with a number. \nExample: 'CallGpt'")]
        [DataRow("Pulse.Lambdas.Gpt13", "core", "CallGpt",
                    "analytics-gpt-chat!", "LambdaName should contain no special characters other than '-'. \nExample: 'analytics-gpt-chat'")]
        public async Task Should_Throw_Error_With_Invalid_Input(string projectName,
                                                                string moduleName,
                                                                string functionName,
                                                                string lambdaName,
                                                                string expectedErrorMessage)
        {
            // 1. Create new Solution
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi(projectName, WebApiFolder, BasePath)).ConfigureAwait(false);

            // 2. Generate lambda
            await Mediator.SendAsync(new GenerateLambda(solutionFile, moduleName, functionName,
                                                        lambdaName, expectedErrorMessage)).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task Should_Throw_Error_With_Invalid_Solution()
        {
            // 1. Create new Solution
            var solutionFile = new FileInfo("NotExistingSolution.sln");

            // 2. Generate lambda
            await Mediator.SendAsync(new GenerateLambda(solutionFile, "core", "CallGpt",
                                                        "analytics-gpt-chat", $"The provided solution file: {solutionFile.FullName} does not exist.")).ConfigureAwait(false);
        }

        internal sealed record GenerateLambda(FileInfo SolutionFile,
                                              string ModuleName,
                                              string FunctionName,
                                              string LambdaName,
                                              string ExpectedErrorMessage = "") : ICommand;

        internal sealed class GenerateLambdaHandler : ICommandHandler<GenerateLambda>
        {
            public async Task Handle(GenerateLambda request,
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

                if (request.ExpectedErrorMessage.IsNotNullOrEmpty())
                {
                    Assert.AreEqual(1, exitCode);
                    Assert.IsTrue(output.Contains(request.ExpectedErrorMessage));
                }
                else
                {
                    Assert.AreEqual(0, exitCode, output);
                }
            }

            private IEnumerable<string> CollectConsoleParameters(GenerateLambda request)
            {
                yield return "runjit";
                yield return "new";
                yield return "lambda";
                yield return "--solution";
                yield return request.SolutionFile.FullName;
                yield return "--module-name";
                yield return request.ModuleName;
                yield return "--function-name";
                yield return request.FunctionName;
                yield return "--lambda-name";
                yield return request.LambdaName;
            }
        }
    }
}
