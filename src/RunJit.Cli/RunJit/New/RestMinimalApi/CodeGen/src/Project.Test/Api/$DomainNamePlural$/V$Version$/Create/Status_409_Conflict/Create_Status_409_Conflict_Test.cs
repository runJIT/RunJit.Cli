using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using Microsoft.AspNetCore.Mvc;
using $ProjectName$.Api.$DomainNamePlural$.V1;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 Create Status 200 OK")]
    public class Create_Status_409_Conflict_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        // SDC Mode:
        // - FormsId is GUID
        // - ProjectId is GUID
        // Pulse Mode:
        // - FormsId is long (SurveyInstanceId)
        // - ProjectId is long
        // FormsType:
        // - OpenAccess
        // - InvitationOnly
        // - AnonymousEmployee
        // - Employee
        [DataTestMethod]
        [DataRow("UseCase_01.json", "UseCase_Conflict.json")]
        [DataRow("Pulse_UseCase_01.json", "Pulse_UseCase_Conflict.json")]
        public async Task Should_Not_Be_Able_To_Create_The_Same_Data_More_Than_Once(string useCase,
                                                                                    string response)
        {
            // 1. Create a new $DomainNameLower$
            await Client.AssertPostAsync<Create$DomainName$Response>("$BasePath$/v1/$DomainNamePluralLower$/",
                                                                           useCase,
                                                                           useCase,
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);

            // 2. Recreate same data must fail with 409 Conflict
            await Client.AssertPostAsErrorAsync<ProblemDetails>("$BasePath$/v1/$DomainNamePluralLower$/",
                                                                useCase,
                                                                response,
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$Unique$DomainName$Name$", Unique$DomainName$Name)]).ConfigureAwait(false);
        }

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?title={Unique$DomainName$Name}");
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
