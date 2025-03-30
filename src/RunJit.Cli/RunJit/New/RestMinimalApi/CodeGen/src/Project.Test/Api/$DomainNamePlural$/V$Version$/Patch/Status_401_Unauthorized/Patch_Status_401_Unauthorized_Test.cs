using AspNetCore.Simple.MsTest.Sdk;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 Patch Status 401 Unauthorized")]
    public class Patch_Status_401_Unauthorized_Test : ApiTestBase
    {
        [TestMethod]
        public Task Should_Not_Be_Able_To_Call_If_Caller_Is_Not_Authorized()
        {
            // Guid.NewGuid() to go sure for missing Auth that we are not patching anything by accident
            return Client.AssertPatchAsUnauthorizedAsync($"$BasePath$/v1/$DomainNamePluralLower$/{Guid.NewGuid()}");
        }
    }
}
