using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Extensions;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit migrate endpoints registration-endpoints")]
    [TestClass]
    public class MigrateEndpointsTest : GlobalSetup
    {
        private const string ApiKey = """
                                      [DynamoDBTable("ApiKey")]
                                      public record ApiKeyEntity
                                      {
                                          [DynamoDBHashKey]
                                          public required string CapabilityId { get; init; }

                                          public string Name { get; init; } = string.Empty;
                                      }
                                      """;

        [TestMethod]
        public async Task Should_Migrate_Endpoints_To_Registration_Endpoints()
        {
            // 1. Create new Web Api
            var solutionFile = await Mediator.SendAsync(new NewMinimalApiProject("Minimal.Api.Endpoints", "api/endpoints")).ConfigureAwait(false);

            // 2. Add rest api
            await Mediator.SendAsync(new NewMinimalRestApi(solutionFile.FullName, ApiKey, "Name",
                                                           "ApiKeys", "api-keys"));

            // 3. After renaming all should be fine if we try to build the solution
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);
        }
    }

    internal sealed record MigrateEndpointsToRegistrationEndpoints(string SolutionFileOrFolder,
                                                                   string OldName,
                                                                   string NewName) : ICommand;

    internal sealed class MigrateEndpointsToRegistrationEndpointsHandler(TestContext testContext) : ICommandHandler<MigrateEndpointsToRegistrationEndpoints>
    {
        public async Task Handle(MigrateEndpointsToRegistrationEndpoints request,
                                 CancellationToken cancellationToken)
        {
            await using var sw = new StringWriter();
            Console.SetOut(sw);

            var strings = CollectConsoleParameters(request).ToArray();
            var consoleCall = strings.Flatten(" ");
            testContext.WriteLine(consoleCall);
            Console.WriteLine();
            Console.WriteLine(consoleCall);
            Debug.WriteLine(consoleCall);
            var exitCode = await Program.Main(strings).ConfigureAwait(false);
            var output = sw.ToString();

            Assert.AreEqual(0, exitCode, output);
        }

        private IEnumerable<string> CollectConsoleParameters(MigrateEndpointsToRegistrationEndpoints parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "migrate";
            yield return "endpoints";
            yield return "register-endpoints";
            yield return parameters.SolutionFileOrFolder;
        }
    }
}
