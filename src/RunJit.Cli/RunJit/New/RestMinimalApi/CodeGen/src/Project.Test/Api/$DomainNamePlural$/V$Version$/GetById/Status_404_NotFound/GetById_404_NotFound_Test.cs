using AspNetCore.Simple.MsTest.Sdk;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 GetById Status 404 NotFound")]
    public class GetBy$IdPropertyName$_404_NotFound_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [DataTestMethod]
        [DataRow("-1", "$IdPropertyName$_Is_Negative_Number.json")]
        [DataRow("invalid-id", "$IdPropertyName$_Is_Invalid.json")]
        [DataRow("f77647b9-b40b-42e5", "$IdPropertyName$_Is_Invalid_Guid.json")]
        public async Task Should_Return_Not_Found_If_$IdPropertyName$_Not_Exist_Or_If_It_Is_Invalid_$IdPropertyName$_Type(string id,
                                                                                              string response)
        {
            // 3. Get all $DomainNamePluralLower$ which matching the name
            await Client.AssertGetAsErrorAsync<ValidationProblemDetailsExtended>($"$BasePath$/v1/$DomainNamePluralLower$/{id}",
                                                                                 response).ConfigureAwait(false);
        }

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertGetAsync($"$BasePath$/v1/$DomainNamePluralLower$?title={Unique$DomainName$Name}");
        }
    }
}
