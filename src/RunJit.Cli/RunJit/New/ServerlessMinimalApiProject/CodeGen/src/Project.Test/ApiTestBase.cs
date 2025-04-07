using System.Runtime.CompilerServices;
using System.Text.Json;
using AspNetCore.Simple.MsTest.Sdk;
using Extensions.Pack;
using Siemens.AspNet.MsTest.Sdk.Aws.Dynamo;

namespace $ProjectName$.Test
{
    /// <summary>
    ///     Provides a base class for API tests, including setup and teardown logic for the test environment.
    /// </summary>
    [TestClass]
    public abstract class ApiTestBase
    {
        /// <summary>
        ///     The base class for API testing, providing utilities for setting up and interacting with the API.
        /// </summary>
        private static ApiTestBase<Program> _apiTestBase = null!;

        protected static HttpClient Client { get; private set; } = null!;

        private static readonly IDynamoDbService DynamoDbService = DynamoDbServiceFactory.Create();

        [AssemblyInitialize]
        public static async Task AssemblyInitializeAsync(TestContext _)
        {
            // 0. Go sure test was not stopped or interrupt
            await AssemblyCleanupAsync().ConfigureAwait(false);

            // 1. Setup dynamo database
            await DynamoDbService.SetupAsync<Program>().ConfigureAwait(false);

            // 2. Setup and load environment variables
            var environmentVariables = EmbeddedFile.GetFileContentFrom("Properties.EnvironmentVariables.json")
                                                   .FromJsonStringAs<Dictionary<string, string>>()
                                                   .Select(keyValue => (keyValue.Key, keyValue.Value)).ToArray();

            // 3. Setup api test base environment
            _apiTestBase = new ApiTestBase<Program>("Development", // The environment name
                                                    (_,
                                                     _) =>
                                                    {
                                                    }, // The register services action
                                                    environmentVariables); // Configure environment variables  

            // 4. We need once the http client to communicate with the started api
            Client = _apiTestBase.CreateClient();

            // 5. Setup test helpers
            var jsonSerializeOptions = _apiTestBase.Services.GetOrThrowMissingException<JsonSerializerOptions>();
            AssertObjectExtensions.JsonSerializerOptions = jsonSerializeOptions;
            HttpClientAssertExtensions.JsonSerializerOptions = jsonSerializeOptions;
        }

        protected static string GetUniqueRunnerName([CallerFilePath] string callerFilePath = "")
        {
            var fileNameWithExtensions = Path.GetFileNameWithoutExtension(callerFilePath);

            return $"{fileNameWithExtensions}_{Environment.MachineName}";
        }

        [AssemblyCleanup]
        public static async Task AssemblyCleanupAsync()
        {
            // 1. Dispose of the API test environment.
            if (_apiTestBase.IsNotNull())
            {
                await _apiTestBase.DisposeAsync().ConfigureAwait(false);
            }

            // 2. Dispose the http client
            if (Client.IsNotNull())
            {
                Client.Dispose();
            }

            // 3. Tear down dynamo database
            await DynamoDbService.TearDownAsync().ConfigureAwait(false);
        }
    }
}
