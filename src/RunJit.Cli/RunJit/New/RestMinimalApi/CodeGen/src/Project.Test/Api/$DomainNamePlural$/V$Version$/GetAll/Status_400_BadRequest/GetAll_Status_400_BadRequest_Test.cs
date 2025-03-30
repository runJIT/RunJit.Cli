using AspNetCore.Simple.MsTest.Sdk;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    /// <summary>
    /// According to 
    /// <see href="https://www.rfc-editor.org/rfc/rfc9110#status.422">
    /// RFC 9110 (Status 422)</see>, the 422 Unprocessable Content status code 
    /// applies only to cases where the request body is syntactically correct 
    /// but semantically invalid (e.g., for POST, PUT, PATCH).
    /// </summary>
    /// <remarks>
    /// Query or URL parameters are not considered part of the request content, 
    /// so for invalid query parameters, a 400 Bad Request is more appropriate.
    /// </remarks>
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 GetAll Status 400 BadRequest")]
    public class GetAll_Status_400_BadRequest_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [DataTestMethod]
        [DataRow("", "Title_Is_Empty.json")]
        [DataRow(" ", "Title_Is_Whitespace.json")]
        public async Task Should_Not_Be_Able_Get_All_$DomainNamePlural$_If_Request_Is_Invalid(string? title,
                                                                                               string response)
        {
            // 3. Get all $DomainNamePluralLower$ which matching the name
            await Client.AssertGetAsErrorAsync<ValidationProblemDetailsExtended>($"$BasePath$/v1/$DomainNamePluralLower$?title={title}",
                                                                                 response).ConfigureAwait(false);
        }

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?title={Unique$DomainName$Name}");
        }
    }
}
