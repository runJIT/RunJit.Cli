using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V$Version$;
using Microsoft.AspNetCore.Mvc;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V$Version$
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V$Version$")]
    public class Delete_$DomainName$_By_$IdPropertyName$_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }
        
        [TestMethod]
        public Task Should_Not_Be_Able_To_Delete_By_$IdPropertyName$_If_Caller_Is_Not_Authorized()
        {
            return Client.AssertDeleteAsUnauthorizedAsync($"api/core/v$Version$/$DomainNamePluralLower$/{Guid.Empty}");
        }

        [TestMethod]
        public Task Should_Not_Be_Able_To_Delete_By_$IdPropertyName$_If_$IdPropertyName$_Is_Not_A_Guid()
        {
            return Client.AssertDeleteAsErrorAsync<ProblemDetails>("api/core/v$Version$/$DomainNamePluralLower$/not-a-guid",
                                                                   "Invalid$IdPropertyName$.json");
        }

        [TestMethod]
        public async Task Should_Be_Able_To_Delete_$DomainName$_By_Id()
        {
            // 1. Create a new $DomainNameLower$
            var create$DomainName$Response = await Client.AssertPostAsync<Create$DomainName$Response>("api/core/v$Version$/$DomainNamePluralLower$/",
                                                                                            "Create$DomainName$.json",
                                                                                            "Create$DomainName$.json",
                                                                                            IgnoreAutoValues,
                                                                                            [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 2. Delete $DomainNameLower$ by id 
            await Client.AssertDeleteAsync($"api/core/v$Version$/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}").ConfigureAwait(false);
            
            // 3. Get all first by $DomainNameLower$ name to go sure already existing data not exists
            await Client.AssertGetAsync<GetAll$DomainNamePlural$Response>($"api/core/v$Version$/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}",
                                                                "No$DomainNamePlural$.json",
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);
        }
        
        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"api/core/v$Version$/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}");
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
