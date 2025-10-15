using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;

namespace $ProjectName$.Test.Api.Health
{
    [TestClass]
    [TestCategory("Health")]
    public class GetHealthStateTest : ApiTestBase
    {
        [TestMethod]
        [DataRow("$BasePath$/health")]
        public Task Should_Return_Healthy_State_Simple_Ok_Check(string route)
        {
            return Client.AssertGetAsync(route);
        }

        [TestMethod]
        [DataRow("$BasePath$/health")]
        public Task Should_Return_Healthy_State(string route)
        {
            return Client.AssertGetAsync<HealthStatusResponse>(route,
                                                               "Healthy.json",
                                                               IgnoreDifferences);
        }

        private IEnumerable<Difference> IgnoreDifferences(IImmutableList<Difference> differences)
        {
            foreach (var difference in differences)
            {
                if (difference.MemberPath.Contains("TotalDuration"))
                {
                    continue;
                }

                yield return difference;
            }
        }
    }

    internal sealed record HealthStatusResponse(string Status,
                                                string TotalDuration,
                                                Dictionary<string, object> Entries);
}
