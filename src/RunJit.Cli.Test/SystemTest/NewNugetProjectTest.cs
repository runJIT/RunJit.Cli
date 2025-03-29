using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Extensions;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit new nuget-project")]
    [TestClass]
    public class NewNugetProjectTest : GlobalSetup
    {
        [DataTestMethod]
        [DataRow("Siemens.AspNet.ErrorHandler")]
        [DataRow("Siemens.AspNet.MinimalApi.Sdk")]
        public async Task Should_Generate_New_Minimal_Web_Api_Solution(string projectName)
        {
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, projectName);

            // 1. Create new solution and projects
            var solutionFileInfo = await Mediator.SendAsync(new NewNugetProject(projectName, targetDirectory)).ConfigureAwait(false);

            // 2. Assert that solution can be build and needed for client as well
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFileInfo.FullName}").ConfigureAwait(false);
        }

        internal sealed record NewNugetProject(string ProjectName,
                                               string TargetDirectory = "",
                                               string ExpectedErrorMessage = "") : ICommand<FileInfo>;

        internal sealed class NewNugetProjectHandler : ICommandHandler<NewNugetProject, FileInfo>
        {
            public async Task<FileInfo> Handle(NewNugetProject request,
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

                // Last output must be the solution file
                var solutionFile = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Last();

                return new FileInfo(solutionFile);
            }

            private IEnumerable<string> CollectConsoleParameters(NewNugetProject request)
            {
                yield return "runjit";
                yield return "new";
                yield return "nuget-project";
                yield return "--project-name";
                yield return request.ProjectName;
                yield return "--target-directory";
                yield return request.TargetDirectory;
                yield return "--start-ide";
            }
        }
    }
}
