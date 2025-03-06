using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using Sdc.Core.Api.Projects.V1;
using Sdc.Core.Api.Projects.V1.Create;
using Sdc.Core.Api.Projects.V1.GetById;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace Sdc.Core.Test.Api.Health
{
    [TestClass]
    [TestCategory("Projects")]
    [TestCategory("Projects V1")]
    public class Get_Project_By_Id_Test : ApiTestBase
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
            return Client.AssertGetAsUnauthorizedAsync("api/core/v1/projects");
        }

        [TestMethod]
        public Task Should_Not_Be_Able_To_Get_By_Id_If_Id_Is_Not_A_Guid()
        {
            return Client.AssertGetAsErrorAsync<ProblemDetails>("api/core/v1/projects/not-a-guid",
                                                                "InvalidId.json");
        }
        
        [TestMethod]
        public async Task Should_Be_Able_To_Get_A_Project_By_Id()
        {
            // 1. Create a new project
            var createProjectResponse = await Client.AssertPostAsync<CreateProjectResponse>("api/core/v1/projects/",
                                                                                            "CreateProject.json",
                                                                                            "CreateProject.json",
                                                                                            differenceFunc: IgnoreAutoValues,
                                                                                            [("$ProjectName$", UniqueName)]).ConfigureAwait(false);

            // 2. Get the currently created project
            await Client.AssertGetAsync<GetProjectByIdResponse>($"api/core/v1/projects/{createProjectResponse.Project.Id}",
                                                                "CreateProject.json",
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$ProjectName$", UniqueName)]).ConfigureAwait(false);

        }
        
        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"api/core/v1/projects?name={UniqueName}");
        }

        private IEnumerable<Difference> IgnoreAutoValues(IImmutableList<Difference> differences)
        {
            foreach (var difference in differences)
            {
                if (difference.MemberPath.Contains($".{nameof(Project.Id)}"))
                {
                    continue;
                }

                yield return difference;
            }
        }
    }
}
