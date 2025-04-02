using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V1;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 DeleteAll Status 200 OK")]
    public class DeleteAll_Status_200_OK_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [DataTestMethod]
        public async Task Should_Be_Able_Delete_All_$DomainNamePlural$_Matching_The_Query_Filter()
        {
            // 0. Get all first by $DomainNameLower$ name to go sure already existing data not exists
            await Client.AssertGetAsync<GetAll$DomainNamePlural$Response>($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}",
                                                                           "No$DomainNamePlural$.json",
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 1. Create a new $DomainNameLower$
            await Client.AssertPostAsync<Create$DomainName$Response>("$BasePath$/v1/$DomainNamePluralLower$/",
                                                                           "UseCase_01.json",
                                                                           "UseCase_01.json",
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 2. Check that the created $DomainNamePluralLower$ exists
            await Client.AssertGetAsync<GetAll$DomainNamePlural$Response>($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}",
                                                                           "GetAll$DomainNamePlural$.json",
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 3. Delete all $DomainNamePluralLower$ which matching the name
            await Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}").ConfigureAwait(false);

            // 4. Eval that all $DomainNamePluralLower$ with the specific name was deleted
            await Client.AssertGetAsync<GetAll$DomainNamePlural$Response>($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}",
                                                                           "No$DomainNamePlural$.json",
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);
        }

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}");
        }

        private IEnumerable<Difference> IgnoreAutoValues(IImmutableList<Difference> differences)
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
