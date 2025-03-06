using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using Sdc.Core.Api.Projects.V1;
using Sdc.Core.Api.Projects.V1.Create;
using Sdc.Core.Api.Projects.V1.GetAll;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace Sdc.Core.Test.Api.Health
{
    [TestClass]
    [TestCategory("Projects")]
    [TestCategory("Projects V1")]
    public class Delete_Project_By_Id_Test : ApiTestBase
    {
        private static readonly string UniqueName = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }
        
        [TestMethod]
        public Task Should_Not_Be_Able_To_Delete_By_Id_If_Caller_Is_Not_Authorized()
        {
            return Client.AssertDeleteAsUnauthorizedAsync($"api/core/v1/projects/{Guid.Empty}");
        }

        [TestMethod]
        public Task Should_Not_Be_Able_To_Delete_By_Id_If_Id_Is_Not_A_Guid()
        {
            return Client.AssertDeleteAsErrorAsync<ProblemDetails>("api/core/v1/projects/not-a-guid",
                                                                   "InvalidId.json");
        }

        [TestMethod]
        public async Task Should_Be_Able_To_Delete_Project_By_Id()
        {
            // 1. Create a new project
            var createProjectResponse = await Client.AssertPostAsync<CreateProjectResponse>("api/core/v1/projects/",
                                                                                            "CreateProject.json",
                                                                                            "CreateProject.json",
                                                                                            IgnoreAutoValues,
                                                                                            [("$ProjectName$", UniqueName)]).ConfigureAwait(false);

            // 2. Delete project by id 
            await Client.AssertDeleteAsync($"api/core/v1/projects/{createProjectResponse.Project.Id}").ConfigureAwait(false);
            
            // 3. Get all first by project name to go sure already existing data not exists
            await Client.AssertGetAsync<GetAllProjectsResponse>($"api/core/v1/projects?name={UniqueName}",
                                                                "NoProjects.json",
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
                if (difference.MemberPath.Contains($"{nameof(Project)}.{nameof(Project.Id)}"))
                {
                    continue;
                }

                yield return difference;
            }
        }
    }
}
