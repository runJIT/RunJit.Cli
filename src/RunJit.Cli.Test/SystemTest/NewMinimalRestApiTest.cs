using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Extensions;
using static RunJit.Cli.Test.SystemTest.NewServerlessMinimalApiTest;

namespace RunJit.Cli.Test.SystemTest
{
    // I need a record Project which have Id, Name and Description as property declaration
    public record Project
    {
        public Guid Id { get; init; } = Guid.Empty;

        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;
    }

    [TestCategory("runjit new minimal-api-serverless")]
    [TestClass]
    public class NewMinimalRestApiTest : GlobalSetup
    {
        // Source gen für migration script
        // 
        //      aws dynamodb create-table \
        //         --table-name Project \
        //         --attribute-definitions AttributeName=ProjectId,AttributeType=S \
        //         --key-schema AttributeName=ProjectId,KeyType=HASH \
        //         --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
        //         --endpoint-url http://localhost:8001

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

        [DataTestMethod]
        //[DataRow("Siemens.Sdc", "api/core", "Sdc")]
        //[DataRow("Siemens.Reporting", "api/reporting", "Reporting")]
        //[DataRow("Pulse.FieldingTool", "api/fieldingtool", "FieldingTool")]
        //[DataRow("Sdc.LandingPage", "api/landingpage", "LandingPage")]
        //[DataRow("Sdc.Console", "api/console", "SdcConsole")]
        [DataRow("Sdc.Core", "api/core", "Core", "Projects", "Name")]
        public async Task Should_Add_New_Rest_Api_Into_Solution(string projectName,
                                                                string basePath,
                                                                string toolName,
                                                                string domainName,
                                                                string queryPropertyName)
        {
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, projectName);

            // 1. Create new solution and projects
            var solutionFileInfo = await Mediator.SendAsync(new NewMinimalApiProject(projectName, basePath, targetDirectory)).ConfigureAwait(false);

            // 2. Add rest api
            await Mediator.SendAsync(new NewMinimalRestApi(ProjectEntityModel, queryPropertyName, domainName, solutionFileInfo.FullName));

            // 2. Assert that solution can be build and needed for client as well
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 3. Assert that solution can be tested
            // await DotNetTool.AssertRunAsync("dotnet", $"test {solutionFileInfo.FullName}").ConfigureAwait(false);
        }

        internal sealed record NewMinimalRestApi(string DbEntityModel,
                                                 string QueryProperty,
                                                 string DomainName,
                                                 string SolutionFile = "",
                                                 string GitRepos = "",
                                                 string WorkingDirectory = "",
                                                 int Version = 1,
                                                 string ExpectedErrorMessage = "") : ICommand<FileInfo>;

        internal sealed class NewMinimalRestApiHandler : ICommandHandler<NewMinimalRestApi, FileInfo>
        {
            public async Task<FileInfo> Handle(NewMinimalRestApi request,
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

            private IEnumerable<string> CollectConsoleParameters(NewMinimalRestApi request)
            {
                yield return "runjit";
                yield return "new";
                yield return "minimal-rest-api";

                if (request.SolutionFile.IsNotNullOrWhiteSpace())
                {
                    yield return "--solution";
                    yield return request.SolutionFile;
                }

                if (request.GitRepos.IsNotNullOrWhiteSpace())
                {
                    yield return "--git-repos";
                    yield return request.GitRepos;
                }

                if (request.WorkingDirectory.IsNotNullOrWhiteSpace())
                {
                    yield return "--working-directory";
                    yield return request.WorkingDirectory;
                }

                yield return "--query-property";
                yield return request.QueryProperty;

                yield return "--version";
                yield return request.Version.ToInvariantString();

                yield return "--domain-name";
                yield return request.DomainName;


                yield return "--entity";
                yield return request.DbEntityModel;

                //yield return $""" "{request.DbEntityModel.Replace("\"", "\"\"")          // Escape double quotes
                //                           .Replace(Environment.NewLine, " ") // Replace Windows newlines with space
                //                           .Replace("\n", " ")                // Replace Unix newlines with space
                //                           .Replace("\r", " ")}" """;          // Just in case
            }
        }
    }
}
