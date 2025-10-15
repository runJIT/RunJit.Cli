using AspNetCore.Simple.MsTest.Sdk;
using Microsoft.AspNetCore.Mvc;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 GetById Status 403 Forbidden")]
    public class GetBy$IdPropertyName$_Status_403_Forbidden_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestMethod]
        [DataRow("devil=TRUNCATE YourTable", "SqlInjection_01.json")] // only one query param invalid
        [DataRow("devil=TRUNCATE YourTable&god=RESTORE YourTable", "SqlInjection_02.json")] // two query param invalid
        public Task Should_Not_Be_Able_Get_All_With_Non_Existing_Query_Parameters(string queryParams,
                                                                                  string response)
        {
            // 3. Get all $DomainNamePluralLower$ which matching the name
            return Client.AssertGetAsErrorAsync<ProblemDetails>($"$BasePath$/v1/$DomainNamePluralLower$/1?{queryParams}",
                                                                response,
                                                                [("$Unique$DomainName$Name$", Unique$DomainName$Name)]);
        }
    }
}
