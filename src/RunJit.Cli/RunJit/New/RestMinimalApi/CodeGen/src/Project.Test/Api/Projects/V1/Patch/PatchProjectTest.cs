using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V1;
using $ProjectName$.Api.$DomainNamePlural$.V1.Create;
using $ProjectName$.Api.$DomainNamePlural$.V1.GetById;
using $ProjectName$.Api.$DomainNamePlural$.V1.Patch;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Test.Api.Health
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1")]
    public class Patch_$DomainName$_Test : ApiTestBase
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
            return Client.AssertPatchAsUnauthorizedAsync("api/core/v1/$DomainNamePluralLower$");
        }

        [TestMethod]
        public Task Should_Not_Be_Able_To_Patch_If_Id_Is_Not_A_Guid()
        {
            return Client.AssertPatchAsync<ProblemDetails>("api/core/v1/$DomainNamePluralLower$/not-a-guid",
                                                           "InvalidId.json");
        }

        [TestMethod]
        public async Task Should_Be_Able_To_Patch_A_$DomainName$_With_Valid_Data()
        {
            // 1. Create a new $DomainNameLower$
            var create$DomainName$Response = await Client.AssertPostAsync<Create$DomainName$Response>("api/core/v1/$DomainNamePluralLower$/",
                                                                                            "Create$DomainName$.json",
                                                                                            "Create$DomainName$.json",
                                                                                            IgnoreAutoValues,
                                                                                            [("$$DomainName$Name$", UniqueName)]).ConfigureAwait(false);

            // 2. Get the currently created $DomainNameLower$
            await Client.AssertGetAsync<Get$DomainName$ByIdResponse>($"api/core/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}",
                                                                "Create$DomainName$.json",
                                                                IgnoreAutoValues,
                                                                [("$$DomainName$Name$", UniqueName)]).ConfigureAwait(false);

            // 3. Patch the existing $DomainNameLower$
            await Client.AssertPatchAsync<Patch$DomainName$Response>($"api/core/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}",
                                                                "Patch$DomainName$.json",
                                                                "Patch$DomainName$.json",
                                                                IgnoreAutoValues,
                                                                [("$$DomainName$Name$", UniqueName)]).ConfigureAwait(false);
        }

        [DataTestMethod]
        [DataRow("InvalidPatch.json", "InvalidPatch.json")]
        public async Task Should_Return_Bad_Request_If_Data_Not_Matching(string request,
                                                                         string response)
        {
            // 1. Create a new $DomainNameLower$
            var create$DomainName$Response = await Client.AssertPostAsync<Create$DomainName$Response>("api/core/v1/$DomainNamePluralLower$/",
                                                                                            "Create$DomainName$.json",
                                                                                            "Create$DomainName$.json",
                                                                                            IgnoreAutoValues,
                                                                                            [("$$DomainName$Name$", UniqueName)],
                                                                                            writeResponse: true).ConfigureAwait(false);

            // 2. Try to patch $DomainNameLower$ with properties which not exists.
            await Client.AssertPatchAsErrorAsync<ValidationProblemDetails>($"api/core/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}",
                                                                           request,
                                                                           response,
                                                                           IgnoreAutoValues,
                                                                           [("$$DomainName$Name$", UniqueName),
                                                                            ("$$DomainName$Id$", create$DomainName$Response.$DomainName$.$IdPropertyName$)]).ConfigureAwait(false);
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
