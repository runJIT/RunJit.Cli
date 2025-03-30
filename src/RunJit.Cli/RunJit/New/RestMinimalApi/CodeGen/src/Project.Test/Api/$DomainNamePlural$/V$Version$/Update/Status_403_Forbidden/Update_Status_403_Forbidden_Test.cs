using AspNetCore.Simple.MsTest.Sdk;
using Microsoft.AspNetCore.Mvc;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 Update Status 403 Forbidden")]
    public class Update_Status_403_Forbidden_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [DataTestMethod]
        [DataRow("devil=TRUNCATE YourTable", "SqlInjection_01.json")]
        [DataRow("devil=TRUNCATE YourTable&god=RESTORE YourTable", "SqlInjection_02.json")]
        public Task Should_Be_Able_To_Update_A_$DomainName$_If_Query_Parameters_Are_Used_Which_Are_Not_Declared(string queryParams,
                                                                                                                      string response)
        {
            return Client.AssertPutAsErrorAsync<ProblemDetails>($"$BasePath$/v1/$DomainNamePluralLower$/1?{queryParams}",
                                                                 "UseCase_01.json",
                                                                 response,
                                                                 [("$Unique$DomainName$Name$", Unique$DomainName$Name)]);
        }

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?title={Unique$DomainName$Name}");
        }
    }
}
