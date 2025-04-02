using AspNetCore.Simple.MsTest.Sdk;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 DeleteAll Status 422 UnprocessableContent")]
    public class DeleteAll_400_BadRequest_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [DataTestMethod]
        [DataRow("", "Title_Is_Empty.json")]
        [DataRow(null, "Title_Is_Null.json")]
        [DataRow(" ", "Title_Is_Whitespace.json")]
        public async Task Should_Not_Be_Able_Delete_All_$DomainNamePlural$_If_Request_Is_Invalid(string title,
                                                                                                  string response)
        {
            // 3. Delete all $DomainNamePluralLower$ which matching the name
            await Client.AssertDeleteAsErrorAsync<ValidationProblemDetailsExtended>($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={title}",
                                                                                    response).ConfigureAwait(false);
        }

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}");
        }
    }
}
