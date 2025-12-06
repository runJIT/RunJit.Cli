using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;
using Solution.Parser.Project;

namespace RunJit.Cli.Generate.DotNetTool.DotNetTool.Test
{
    internal static class AddGlobalSetupCodeGenExtension
    {
        internal static void AddGlobalSetupCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolTestSpecificCodeGen, GlobalSetupCodeGen>();
        }
    }

    internal sealed class GlobalSetupCodeGen(ConsoleService consoleService) : IDotNetToolTestSpecificCodeGen
    {
        private const string Template = """
                                        using AspNetCore.Simple.MsTest.Sdk;
                                        using DotNetTool.Service;
                                        using Extensions.Pack;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            /// <summary>
                                            ///     Provides global setup and teardown logic for the test environment.
                                            /// </summary>
                                            [TestClass]
                                            public class GlobalSetup
                                            {
                                                /// <summary>
                                                ///     The name of the Docker container used for DynamoDB during tests.
                                                /// </summary>
                                                private const string DynamoDbContainerName = "dynamodb-local";

                                                /// <summary>
                                                ///     The HTTP client used for communicating with the API during tests.
                                                /// </summary>
                                                private static HttpClient? _httpClient;

                                                /// <summary>
                                                ///     The base class for API testing, providing utilities for setting up and interacting with the API.
                                                /// </summary>
                                                private static ApiTestBase<$webApiProjectName$.Program> _apiTestBase = null!;

                                                /// <summary>
                                                ///     The tool used for executing .NET commands, such as managing Docker containers.
                                                /// </summary>
                                                private static readonly IDotNetTool DotNetTool = DotNetToolFactory.Create();

                                                /// <summary>
                                                ///     Provides access to the service provider for dependency injection.
                                                /// </summary>
                                                protected static IServiceProvider Services { get; private set; } = null!;

                                                /// <summary>
                                                ///     Provides access to the CLI runner for executing CLI commands.
                                                /// </summary>
                                                protected static CliRunner Cli { get; private set; } = null!;

                                                /// <summary>
                                                ///     Initializes the test environment by setting up the service provider, CLI runner, and other dependencies.
                                                ///     This method is called once before any tests are executed.
                                                /// </summary>
                                                /// <param name="testContext">The context of the test run.</param>
                                                [AssemblyInitialize]
                                                public static async Task InitAsync(TestContext testContext)
                                                {
                                                    // 1. Start the required Docker container in debug mode.
                                                    if (typeof(GlobalSetup).Assembly.IsCompiledInDebug())
                                                    {
                                                        var dockerRunResult = await DotNetTool.RunAsync("docker", $"run -d -p 8001:8000 --name {DynamoDbContainerName} amazon/{DynamoDbContainerName} -jar DynamoDBLocal.jar -sharedDb").ConfigureAwait(false);
                                                        Assert.AreEqual(0, dockerRunResult.ExitCode, $"Dynamo DB: {DynamoDbContainerName} could not be started. Please check if you have Docker installed.");
                                                    }

                                                    // 2. Load environment variables from an embedded JSON file.
                                                    var environmentVariables = EmbeddedFile.GetFileContentFrom("Properties.EnvironmentVariables.json")
                                                                                           .FromJsonStringAs<Dictionary<string, string>>()
                                                                                           .Select(keyValue => (keyValue.Key, keyValue.Value))
                                                                                           .ToArray();

                                                    // 3. Set up the API test base environment.
                                                    _apiTestBase = new ApiTestBase<$webApiProjectName$.Program>("Development", // The environment name
                                                                                                            (_,
                                                                                                             _) =>
                                                                                                            {
                                                                                                            }, // The register services action
                                                                                                            environmentVariables // Configure environment variables
                                                                                                           );

                                                    // 4. Create an HTTP client for communicating with the API.
                                                    _httpClient = _apiTestBase.CreateClient();

                                                    // 6. Create a new service collection for dependency injection.
                                                    var serviceCollection = new ServiceCollection();

                                                    // 7. Register the test context and DotNetTool as singletons.
                                                    serviceCollection.AddSingleton(testContext);
                                                    serviceCollection.AddSingleton(DotNetTool);

                                                    // 8. Register the CLI runner extension.
                                                    serviceCollection.AddCliRunner();

                                                    // 9. Build the service provider from the service collection.
                                                    var serviceProvider = serviceCollection.BuildServiceProvider();

                                                    // 10. Assign the service provider and CLI runner to the static properties.
                                                    Services = serviceProvider;
                                                    Cli = new CliRunner(_httpClient);
                                                }

                                                /// <summary>
                                                ///     Cleans up the test environment after all tests have been executed.
                                                /// </summary>
                                                [AssemblyCleanup]
                                                public static async Task AssemblyCleanupAsync()
                                                {
                                                    // 1. Dispose of the API test environment.
                                                    if (_apiTestBase.IsNotNull())
                                                    {
                                                        await _apiTestBase.DisposeAsync().ConfigureAwait(false);
                                                    }

                                                    // 2. Dispose of the HTTP client.
                                                    _httpClient?.Dispose();

                                                    // 3. Stop and remove the Docker container in debug mode to ensure a clean state.
                                                    if (typeof(GlobalSetup).Assembly.IsCompiledInDebug())
                                                    {
                                                        await DotNetTool.RunAsync("docker", $"stop {DynamoDbContainerName}").ConfigureAwait(false);
                                                        await DotNetTool.RunAsync("docker", $"rm {DynamoDbContainerName}").ConfigureAwait(false);
                                                    }
                                                }
                                            }
                                        }
                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos,
                                        ProjectFile? webApiProject)
        {
            // 1. GlobalSetup
            var file = Path.Combine(projectFileInfo.Directory!.FullName, "GlobalSetup.cs");

            var newTemplate = Template.Replace("$namespace$", $"{dotNetToolInfos.ProjectName}.Test")
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName)
                                      .Replace("$webApiProjectName$", webApiProject?.ProjectFileInfo.FileNameWithoutExtenion);

            var formattedTemplate = newTemplate.FormatSyntaxTree();

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 2. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
