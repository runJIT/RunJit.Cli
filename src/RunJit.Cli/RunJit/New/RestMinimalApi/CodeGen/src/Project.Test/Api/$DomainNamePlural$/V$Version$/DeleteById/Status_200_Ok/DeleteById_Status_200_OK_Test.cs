using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using Microsoft.AspNetCore.Mvc;
using $ProjectName$.Api.$DomainNamePlural$.V1;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 DeleteById Status 200 OK")]
    public class DeleteBy$IdPropertyName$_Status_200_OK_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [TestMethod]
        public async Task Should_Be_Able_Delete_$DomainNamePlural$_By_Id()
        {
            // 1. Get all first by $DomainNameLower$ name to go sure already existing data not exists
            await Client.AssertGetAsync<GetAll$DomainNamePlural$Response>($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}",
                                                                           "No$DomainNamePlural$.json",
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 2. Create a new $DomainNameLower$
            var create$DomainName$Response = await Client.AssertPostAsync<Create$DomainName$Response>("$BasePath$/v1/$DomainNamePluralLower$/",
                                                                                                                  "UseCase_01.json",
                                                                                                                  "UseCase_01.json",
                                                                                                                  differenceFunc: IgnoreAutoValues,
                                                                                                                  [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 3. Evaluate that the created $DomainNameLower$ is available
            await Client.AssertGetAsync<Get$DomainName$ByIdResponse>($"$BasePath$/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}",
                                                                           "UseCase_01.json",
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [
                                                                               ("$FormsId$", create$DomainName$Response.$DomainName$.$IdPropertyName$),
                                                                               ("$Unique$DomainName$Name$", Unique$DomainName$Name)
                                                                           ]).ConfigureAwait(false);

            // 3. Delete $DomainNamePluralLower$ by id which matching the name
            await Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}").ConfigureAwait(false);

            // 4. Eval that all $DomainNamePluralLower$ with the specific id does not exist anymore
            await Client.AssertGetAsErrorAsync<ProblemDetails>($"$BasePath$/v1/$DomainNamePluralLower$/{create$DomainName$Response.$DomainName$.$IdPropertyName$}",
                                                               "NotFound.json",
                                                               differenceFunc: IgnoreAutoValues,
                                                               [
                                                                   ("$FormsId$", create$DomainName$Response.$DomainName$.$IdPropertyName$),
                                                                   ("$Unique$DomainName$Name$", Unique$DomainName$Name)
                                                               ]).ConfigureAwait(false);
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
