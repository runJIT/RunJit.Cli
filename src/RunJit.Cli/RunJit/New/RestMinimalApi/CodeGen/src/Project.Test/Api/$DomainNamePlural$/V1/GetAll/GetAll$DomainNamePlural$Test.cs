using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V1;
using $ProjectName$.Api.$DomainNamePlural$.V1.Create;
using $ProjectName$.Api.$DomainNamePlural$.V1.GetAll;

namespace $ProjectName$.Test.Api.Health
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1")]
    public class Get_All_$DomainNamePlural$_Test : ApiTestBase
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
            return Client.AssertGetAsUnauthorizedAsync("api/core/v1/$DomainNamePluralLower$");
        }

        [TestMethod]
        public async Task Should_Be_Able_To_Get_All_$DomainNamePlural$_Matching_The_Query_Filter()
        {
            // 0. Get all first by $DomainNameLower$ name to go sure already existing data not exists
            await Client.AssertGetAsync<GetAll$DomainNamePlural$Response>($"api/core/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={UniqueName}",
                                                                "No$DomainNamePlural$.json",
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$$DomainName$Name$", UniqueName)]).ConfigureAwait(false);
            
            // 1. Create a new $DomainNameLower$ (1)
            await Client.AssertPostAsync<Create$DomainName$Response>("api/core/v1/$DomainNamePluralLower$/",
                                                                "Create$DomainName$.json",
                                                                "Create$DomainName$.json",
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$$DomainName$Name$", UniqueName)]).ConfigureAwait(false);
            
            // 2. Create a new $DomainNameLower$ (2)
            await Client.AssertPostAsync<Create$DomainName$Response>("api/core/v1/$DomainNamePluralLower$/",
                                                                "Create$DomainName$.json",
                                                                "Create$DomainName$.json",
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$$DomainName$Name$", UniqueName)]).ConfigureAwait(false);

            // 4. Eval that all $DomainNamePluralLower$ with the specific name was added
            await Client.AssertGetAsync<GetAll$DomainNamePlural$Response>($"api/core/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={UniqueName}",
                                                                "GetAll$DomainNamePlural$.json",
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$$DomainName$Name$", UniqueName)]).ConfigureAwait(false);

        }
        
        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"api/core/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={UniqueName}");
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
