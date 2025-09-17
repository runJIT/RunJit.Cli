using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Extensions;
using static RunJit.Cli.Test.SystemTest.NewMinimalRestApiTest;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit new minimal-api-serverless")]
    [TestClass]
    public class NewServerlessMinimalApiTest : GlobalSetup
    {
        [TestMethod]
        [DataRow("Siemens.Sdc", "api/core", "Sdc")]
        [DataRow("Siemens.Reporting", "api/reporting", "Reporting")]
        [DataRow("Pulse.FieldingTool", "api/fieldingtool", "FieldingTool")]
        [DataRow("Sdc.LandingPage", "api/landingpage", "LandingPage")]
        [DataRow("Sdc.Console", "api/console", "SdcConsole")]
        [DataRow("Sdc.Core", "api/core", "Core")]
        public async Task Should_Generate_New_Minimal_Web_Api_Solution(string projectName,
                                                                       string basePath,
                                                                       string toolName)
        {
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, projectName);

            // 1. Create new solution and projects
            var solutionFileInfo = await Mediator.SendAsync(new NewMinimalApiProject(projectName, basePath, targetDirectory)).ConfigureAwait(false);

            // 2. Assert that solution can be build and needed for client as well
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 3. Assert that solution can be tested
            // await DotNetTool.AssertRunAsync("dotnet", $"test {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 4. Create Client
            await Mediator.SendAsync(new GenerateClient(solutionFileInfo, false));

            // 5. Create .Net tool
            await Mediator.SendAsync(new GenerateDotNetTool(solutionFileInfo, toolName));

            // 6. Add code rules
            await Mediator.SendAsync(new UpdateCodeRulesForSolution(solutionFileInfo.FullName)).ConfigureAwait(false);

            // 6.Assert that solution can be build
            // await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 7.Assert that solution can be tested
            // await DotNetTool.AssertRunAsync("dotnet", $"test {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 8. Pack
            //  wait DotNetTool.AssertRunAsync("dotnet", $@"pack {solutionFileInfo.FullName} -o D:\Nuget").ConfigureAwait(false);

            // 9. Publish
            // await DotNetTool.AssertRunAsync("dotnet", $@"pack {solutionFileInfo.FullName} -o D:\Nuget").ConfigureAwait(false);
        }

        private const string ProjectEntityModel = """
                                                  [DynamoDBTable("Project")]
                                                  public record ProjectEntity
                                                  {
                                                      [DynamoDBHashKey]
                                                      public Guid ProjectId { get; init; } = Guid.Empty;

                                                      public string Name { get; init; } = string.Empty;

                                                      public string Description { get; init; } = string.Empty;
                                                  }
                                                  """;

        [TestMethod]
        [DataRow("Siemens.Sdc", "api/core", "Sdc")]
        [DataRow("Siemens.Reporting", "api/reporting", "Reporting")]
        [DataRow("Pulse.FieldingTool", "api/fieldingtool", "FieldingTool")]
        [DataRow("Sdc.LandingPage", "api/landingpage", "LandingPage")]
        [DataRow("Sdc.Console", "api/console", "SdcConsole")]
        [DataRow("Sdc.Core", "api/core", "Core")]
        public async Task Should_Generate_New_Minimal_Web_Api_Solution_With_Api_Endpoint(string projectName,
                                                                                         string basePath,
                                                                                         string toolName)
        {
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, projectName);

            // 1. Create new solution and projects
            var solutionFileInfo = await Mediator.SendAsync(new NewMinimalApiProject(projectName, basePath, targetDirectory)).ConfigureAwait(false);

            // 2. Add rest api
            await Mediator.SendAsync(new NewMinimalRestApi(solutionFileInfo.FullName, ProjectEntityModel, "Name", "Projects", basePath));

            // 3. Assert that solution can be build and needed for client as well
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 4. Create Client
            await Mediator.SendAsync(new GenerateClient(solutionFileInfo, false));

            // 5. Create .Net tool
            await Mediator.SendAsync(new GenerateDotNetTool(solutionFileInfo, toolName));

            // 6. Add code rules
            await Mediator.SendAsync(new UpdateCodeRulesForSolution(solutionFileInfo.FullName)).ConfigureAwait(false);

            // 6.Assert that solution can be build
            // await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 7.Assert that solution can be tested
            // await DotNetTool.AssertRunAsync("dotnet", $"test {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 8. Pack
            //  wait DotNetTool.AssertRunAsync("dotnet", $@"pack {solutionFileInfo.FullName} -o D:\Nuget").ConfigureAwait(false);

            // 9. Publish
            // await DotNetTool.AssertRunAsync("dotnet", $@"pack {solutionFileInfo.FullName} -o D:\Nuget").ConfigureAwait(false);
        }
    }

    internal sealed record NewMinimalApiProject(string ProjectName,
                                            string basePath,
                                            string TargetDirectory = "",
                                            string ExpectedErrorMessage = "") : ICommand<FileInfo>;

    internal sealed class NewMinimalApiProjectHandler : ICommandHandler<NewMinimalApiProject, FileInfo>
    {
        public async Task<FileInfo> Handle(NewMinimalApiProject request,
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

        private IEnumerable<string> CollectConsoleParameters(NewMinimalApiProject request)
        {
            yield return "runjit";
            yield return "new";
            yield return "minimal-api-serverless";
            yield return "--project-name";
            yield return request.ProjectName;
            yield return "--base-path";
            yield return request.basePath;

            if (request.TargetDirectory.IsNotNullOrWhiteSpace())
            {
                yield return "--target-directory";
                yield return request.TargetDirectory;
            }
        }
    }
}
