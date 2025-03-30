using AspNetCore.Simple.MsTest.Sdk;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 GetAll Status 401 Unauthorized")]
    public class GetAll_Status_401_Unauthorized_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();
        
        [TestMethod]
        public Task Should_Not_Be_Able_To_Call_If_Caller_Is_Not_Authorized()
        {
            return Client.AssertGetAsUnauthorizedAsync($"$BasePath$/v1/$DomainNamePluralLower$?filter={Unique$DomainName$Name}");
        }
    }
}
