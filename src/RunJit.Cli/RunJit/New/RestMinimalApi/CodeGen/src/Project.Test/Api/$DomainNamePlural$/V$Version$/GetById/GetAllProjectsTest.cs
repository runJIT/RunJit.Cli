using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V$Version$;
using Microsoft.AspNetCore.Mvc;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V$Version$
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V$Version$")]
    public class Get_$DomainName$_By_$IdPropertyName$_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();
        
        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }
        
        [TestMethod]
        public Task Should_Not_Be_Able_To_Call_If_Caller_Is_Not_Authorized()
        {
            return Client.AssertGetAsUnauthorizedAsync("api/core/v$Version$/$DomainNamePluralLower$");
        }

        [TestMethod]
        public Task Should_Not_Be_Able_To_Get_By_$IdPropertyName$_If_$IdPropertyName$_Is_Not_A_Guid()
        {
            return Client.AssertGetAsErrorAsync<ProblemDetails>("api/core/v$Version$/$DomainNamePluralLower$/not-a-guid",
                                                                "Invalid$IdPropertyName$.json");
        }
        
        [TestMethod]
        public async Task Should_Be_Able_To_Get_A_$DomainName$_By_Id()
        {
            // 1. Create a new $DomainNameLower$
            var create$DomainName$Response = await Client.AssertPostAsync<Create$DomainName$Response>("api/core/v$Version$/$DomainNamePluralLower$/",
                                                                                            "Create$DomainName$.json",
                                                                                            "Create$DomainName$.json",
                                                                                            differenceFunc: IgnoreAutoValues,
                                                                                            [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 2. Get the currently created $DomainNameLower$
            await Client.AssertGetAsync<Get$DomainName$ByIdResponse>($"api/core/v$Version$/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}",
                                                                "Create$DomainName$.json",
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
                if (difference.MemberPath.Contains($".{nameof($DomainName$.$IdPropertyName$)}"))
                {
                    continue;
                }

                yield return difference;
            }
        }
    }
}
