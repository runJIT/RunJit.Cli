using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V1;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    /// <summary>
    /// For request bodies that are syntactically correct yet semantically invalid (e.g., in POST, PUT, PATCH), 
    /// a 422 Unprocessable Content status is recommended 
    /// (<see href="https://www.rfc-editor.org/rfc/rfc9110#status.422">RFC 9110</see>).
    /// </summary>
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 Patch Status 422 UnprocessableContent")]
    public class Patch_Status_422_UnprocessableContent_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [TestMethod]
        [DynamicRequestLocator]
        public async Task Should_Return_Unprocessable_Entity_If_Patch_Request_Data_Contains_Invalid(string useCase)
        {
            // 1. Create an initial $DomainNameLower$ which we can patch (baseline)
            var create$DomainName$Response = await Client.AssertPostAsync<Create$DomainName$Response>("$BasePath$/v1/$DomainNamePluralLower$",
                                                                                                                  "Sdc_Baseline.json",
                                                                                                                  "Sdc_Baseline.json",
                                                                                                                  differenceFunc: IgnoreAutoValues,
                                                                                                                  [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 2. Assert that validation error occured
            
            await Client.AssertPatchAsErrorAsync<ValidationProblemDetailsExtended>($"$BasePath$/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}",
                                                                                   useCase,
                                                                                   useCase,
                                                                                   differenceFunc: IgnoreAutoValues,
                                                                                   [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);
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

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}");
        }
    }
}
