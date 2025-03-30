using AspNetCore.Simple.MsTest.Sdk;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 GetById Status 401 Unauthorized")]
    public class GetBy$IdPropertyName$_Status_401_Unauthorized_Test : ApiTestBase
    {
        [TestMethod]
        public Task Should_Not_Be_Able_To_Call_If_Caller_Is_Not_Authorized()
        {
            // Guid.NewGuid() to go sure we are not getting any data -> not found
            return Client.AssertGetAsUnauthorizedAsync($"$BasePath$/v1/$DomainNamePluralLower$/{Guid.NewGuid()}");
        }
    }
}
