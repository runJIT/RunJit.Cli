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
    [TestCategory("$DomainNamePlural$ V1 Update Status 422 UnprocessableContent")]
    public class Update_Status_422_UnprocessableContent_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [DataTestMethod]
        [DynamicRequestLocator]
        public Task Should_Return_Unprocessable_Entity_If_Update_Request_Data_Contains_Invalid(string useCase)
        {
            return Client.AssertPutAsErrorAsync<ValidationProblemDetailsExtended>($"$BasePath$/v1/$DomainNamePluralLower$/1",
                                                                                  useCase,
                                                                                  useCase,
                                                                                  differenceFunc: IgnoreAutoValues,
                                                                                  [("$Unique$DomainName$Name$", Unique$DomainName$Name)]);
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

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}");
        }
    }
}
