using AspNetCore.Simple.MsTest.Sdk;

namespace $ProjectName$.Test.OpenApi
{
    [TestClass]
    [TestCategory("OpenApi")]
    public class OpenApiTest : ApiTestBase
    {
        [TestMethod]
        public async Task Should_Return_Open_Api_Json_For_Each_Version()
        {
            // ToDo: We need version provider / collector to know all versions
            //       Or we use the generated static versions :) which is needed fot Native AOT
            var versions = new[] { 1 };

            foreach (var version in versions)
            {
                await Client.AssertGetAsync<object>($"openapi/v{version}.json",
                                                    $"V{version}.json").ConfigureAwait(false);
            }
        }
    }
}
