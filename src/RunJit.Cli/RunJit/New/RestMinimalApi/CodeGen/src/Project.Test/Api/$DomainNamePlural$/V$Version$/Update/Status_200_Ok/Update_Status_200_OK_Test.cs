using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V1;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 Update Status 200 OK")]
    public class UpdateStatus200OkTest : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [DataTestMethod]
        // Full object Put like update
        [DataRow("UseCase_01.json")]
        public async Task Should_Be_Able_To_Update_A_$DomainName$(string useCase)
        {
            // 1. Create an initial $DomainNameLower$ which we can Put (baseline)
            var create$DomainName$Response = await Client.AssertPostAsync<Create$DomainName$Response>("$BasePath$/v1/$DomainNamePluralLower$/",
                                                                                                                  "UseCase_Baseline",
                                                                                                                  "UseCase_Baseline",
                                                                                                                  differenceFunc: IgnoreAutoValues,
                                                                                                                  [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 2. Put the created $DomainNameLower$ which we created before
            await Client.AssertPutAsync<Update$DomainName$Response>($"$BasePath$/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}",
                                                                           useCase,
                                                                           useCase,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name),
                                                                            ("$FormsId$", create$DomainName$Response.$DomainName$.$IdPropertyName$)]).ConfigureAwait(false);

            // 2. Get the currently Puted $DomainNameLower$ to check if all values are correct
            await Client.AssertGetAsync<Get$DomainName$ByIdResponse>($"$BasePath$/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}",
                                                                           useCase,
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
