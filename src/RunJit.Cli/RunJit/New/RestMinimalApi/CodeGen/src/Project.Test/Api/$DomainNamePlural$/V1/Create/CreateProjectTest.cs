using System.Collections.Immutable;
using System.Reflection;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V1;
using $ProjectName$.Api.$DomainNamePlural$.V1.Create;
using $ProjectName$.Api.$DomainNamePlural$.V1.GetById;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Test.Api.Health
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1")]
    public class Create_$DomainName$_Test : ApiTestBase
    {
        private static readonly string UniqueName = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [TestMethod]
        public Task Should_Not_Be_Able_To_Call_If_Caller_Is_Not_Authorized()
        {
            return Client.AssertGetAsUnauthorizedAsync($"api/core/v1/$DomainNamePluralLower$");
        }


        [DataTestMethod]
        [DataRow("Invalid$DomainName$.json", "Invalid$DomainName$.json")]
        public Task Should_Return_Bad_Request_If_Create_Request_Data_Are_Invalid(string request, string response)
        {
            return Client.AssertPatchAsErrorAsync<ValidationProblemDetails>($"api/core/v1/$DomainNamePluralLower$/{Guid.Empty}",
                                                                            request,
                                                                            response);

        }


        [TestMethod]
        public async Task Should_Be_Able_To_Create_A_$DomainName$_With_Valid_Data()
        {
            // 1. Create a new $DomainNameLower$
            var create$DomainName$Response = await Client.AssertPostAsync<Create$DomainName$Response>("api/core/v1/$DomainNamePluralLower$/",
                                                                "Create$DomainName$.json",
                                                                "Create$DomainName$.json",
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$$DomainName$Name$", UniqueName)]).ConfigureAwait(false);

            // 2. Get the currently created $DomainNameLower$
            await Client.AssertGetAsync<Get$DomainName$ByIdResponse>($"api/core/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}",
                                                                "Create$DomainName$.json",
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$$DomainName$Name$", UniqueName)]).ConfigureAwait(false);

        }

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"api/core/v1/$DomainNamePluralLower$?name={UniqueName}");
        }

        private IEnumerable<Difference> IgnoreAutoValues(IImmutableList<Difference> differences)
        {
            foreach (var difference in differences)
            {
                if (difference.MemberPath.Contains($".{nameof($DomainName$.$IdPropertyName$)}"))
                {
                    continue;
                }

                yield return difference;
            }
        }
    }
}
