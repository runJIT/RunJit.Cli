using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V1;
using $ProjectName$.Api.$DomainNamePlural$.V1.Create;
using $ProjectName$.Api.$DomainNamePlural$.V1.GetById;
using $ProjectName$.Api.$DomainNamePlural$.V1.Update;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Test.Api.Health
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1")]
    public class Update_$DomainName$_Test : ApiTestBase
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
            return Client.AssertPutAsUnauthorizedAsync($"api/core/v1/$DomainNamePluralLower$");
        }

        [TestMethod]
        public Task Should_Not_Be_Able_To_Patch_If_$IdPropertyName$_Is_Not_A_Guid()
        {
            return Client.AssertPatchAsync<ProblemDetails>("api/core/v1/$DomainNamePluralLower$/not-a-guid",
                                                           "Invalid$IdPropertyName$.json");
        }
        

        [DataTestMethod]
        [DataRow("Invalid$DomainName$.json", "Invalid$DomainName$.json")]
        public Task Should_Return_Bad_Request_If_Update_Request_Data_Are_Invalid(string request, string response)
        {
            return Client.AssertPutAsErrorAsync<ValidationProblemDetails>("api/core/v1/$DomainNamePluralLower$/",
                                                                            request,
                                                                            response);

        }


        [TestMethod]
        public async Task Should_Be_Able_To_Update_A_$DomainName$_With_Valid_Data()
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
            
            // 3. Update the existing $DomainNameLower$
            await Client.AssertPutAsync<Update$DomainName$Response>($"api/core/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}",
                                                               "Update$DomainName$.json",
                                                               "Update$DomainName$.json",
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
