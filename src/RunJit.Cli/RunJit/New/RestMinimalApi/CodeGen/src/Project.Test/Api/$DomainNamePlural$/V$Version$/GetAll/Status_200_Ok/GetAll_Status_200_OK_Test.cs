using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V1;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 GetAll Status 200 OK")]
    public class GetAll_Status_200_OK_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [TestMethod]
        [DataRow("")] // Query filters which are not declared in the route are NULL
        [DataRow("?$QueryPropertyNameLower$=1")] // Query filters which are not declared in the route are NULL
        [DataRow("?$QueryPropertyNameLower$=Hello")] // Query filters which are not declared in the route are NULL
        public Task Should_Be_Able_To_Get_All_$DomainNamePlural$_Without_Any_Filters(string validQueryFilters)
        {
            // 4. Eval that all $DomainNamePluralLower$ with the specific name was added
            return Client.AssertGetAsync($"$BasePath$/v1/$DomainNamePluralLower${validQueryFilters}");
        }
        
        
        [TestMethod]
        public async Task Should_Be_Able_To_Get_All_$DomainNamePlural$_Matching_The_Query_Filter()
        {
            // 0. Get all first by $DomainNameLower$ name to go sure already existing data not exists
            await Client.AssertGetAsync<GetAll$DomainNamePlural$Response>($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}",
                                                                           "No$DomainNamePlural$.json",
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 1. Create a new $DomainNameLower$ (1)
            await Client.AssertPostAsync<Create$DomainName$Response>("$BasePath$/v1/$DomainNamePluralLower$/",
                                                                           "UseCase_01.json",
                                                                           "UseCase_01.json",
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 2. Create a new $DomainNameLower$ (2)
            await Client.AssertPostAsync<Create$DomainName$Response>("$BasePath$/v1/$DomainNamePluralLower$/",
                                                                           "UseCase_02.json",
                                                                           "UseCase_02.json",
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 4. Eval that all $DomainNamePluralLower$ with the specific name was added
            await Client.AssertGetAsync<GetAll$DomainNamePlural$Response>($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}",
                                                                           "GetAll$DomainNamePlural$.json",
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);
        }

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}");
        }

        private IEnumerable<Difference> IgnoreAutoValues(ImmutableList<Difference> differences)
        {
            foreach (var difference in differences)
            {
                if (difference.MemberPath.Contains($".{nameof($DomainName$.$IdPropertyName$)}", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return difference;
            }
        }
    }
}
