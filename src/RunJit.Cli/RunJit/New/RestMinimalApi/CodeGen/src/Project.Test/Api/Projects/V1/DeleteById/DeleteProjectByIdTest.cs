using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V1;
using $ProjectName$.Api.$DomainNamePlural$.V1.Create;
using $ProjectName$.Api.$DomainNamePlural$.V1.GetAll;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Test.Api.Health
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1")]
    public class Delete_$DomainName$_By_Id_Test : ApiTestBase
    {
        private static readonly string UniqueName = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }
        
        [TestMethod]
        public Task Should_Not_Be_Able_To_Delete_By_Id_If_Caller_Is_Not_Authorized()
        {
            return Client.AssertDeleteAsUnauthorizedAsync($"api/core/v1/$DomainNamePluralLower$/{Guid.Empty}");
        }

        [TestMethod]
        public Task Should_Not_Be_Able_To_Delete_By_Id_If_Id_Is_Not_A_Guid()
        {
            return Client.AssertDeleteAsErrorAsync<ProblemDetails>("api/core/v1/$DomainNamePluralLower$/not-a-guid",
                                                                   "InvalidId.json");
        }

        [TestMethod]
        public async Task Should_Be_Able_To_Delete_$DomainName$_By_Id()
        {
            // 1. Create a new $DomainNameLower$
            var create$DomainName$Response = await Client.AssertPostAsync<Create$DomainName$Response>("api/core/v1/$DomainNamePluralLower$/",
                                                                                            "Create$DomainName$.json",
                                                                                            "Create$DomainName$.json",
                                                                                            IgnoreAutoValues,
                                                                                            [("$$DomainName$Name$", UniqueName)]).ConfigureAwait(false);

            // 2. Delete $DomainNameLower$ by id 
            await Client.AssertDeleteAsync($"api/core/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}").ConfigureAwait(false);
            
            // 3. Get all first by $DomainNameLower$ name to go sure already existing data not exists
            await Client.AssertGetAsync<GetAll$DomainNamePlural$Response>($"api/core/v1/$DomainNamePluralLower$?name={UniqueName}",
                                                                "No$DomainNamePlural$.json",
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
                if (difference.MemberPath.Contains($"{nameof($DomainName$)}.{nameof($DomainName$.$IdPropertyName$)}"))
                {
                    continue;
                }

                yield return difference;
            }
        }
    }
}
