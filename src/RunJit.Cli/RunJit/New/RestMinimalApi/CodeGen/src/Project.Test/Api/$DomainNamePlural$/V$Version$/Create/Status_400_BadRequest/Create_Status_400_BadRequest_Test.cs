using AspNetCore.Simple.MsTest.Sdk;
using Microsoft.AspNetCore.Mvc;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 Create Status 400 BadRequest")]
    public class Create_Status_400_BadRequest_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [DataTestMethod]
        [DataRow("InvalidJson_01.json")]
        public Task Should_Return_Bad_Request_When_Trying_To_Create_The_$DomainName$_With(string useCase)
        {
            return Client.AssertPostAsErrorAsync<ProblemDetails>($"$BasePath$/v1/$DomainNamePluralLower$",
                                                                 useCase,
                                                                 useCase,
                                                                 [("$Unique$DomainName$Name$", Unique$DomainName$Name)]);
        }

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}");
        }
    }
}
