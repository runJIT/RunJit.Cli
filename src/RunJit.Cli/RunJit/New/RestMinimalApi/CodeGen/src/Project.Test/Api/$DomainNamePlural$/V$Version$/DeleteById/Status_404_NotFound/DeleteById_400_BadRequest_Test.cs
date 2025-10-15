using AspNetCore.Simple.MsTest.Sdk;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 DeleteById Status 400 BadRequest")]
    public class DeleteBy$IdPropertyName$_400_BadRequest_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [TestMethod]
        [DataRow("-1", "$IdPropertyName$_Is_Negative_Number.json")]
        [DataRow("invalid-id", "$IdPropertyName$_Is_Invalid.json")]
        [DataRow("f77647b9-b40b-42e5", "$IdPropertyName$_Is_Invalid_Guid.json")]
        public async Task Should_Return_Not_Found_If_$IdPropertyName$_Not_Exist_Or_If_It_Is_Invalid_$IdPropertyName$_Type(string id,
                                                                                              string response)
        {
            // 3. Delete all $DomainNamePluralLower$ which matching the name
            await Client.AssertDeleteAsErrorAsync<ValidationProblemDetailsExtended>($"$BasePath$/v1/$DomainNamePluralLower$/{id}",
                                                                                    response).ConfigureAwait(false);
        }

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}");
        }
    }
}
