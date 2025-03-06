using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using Sdc.Core.Api.Projects.V1;
using Sdc.Core.Api.Projects.V1.Create;
using Sdc.Core.Api.Projects.V1.GetAll;

namespace Sdc.Core.Test.Api.Health
{
    [TestClass]
    [TestCategory("Projects")]
    [TestCategory("Projects V1")]
    public class Get_All_Projects_Test : ApiTestBase
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
        public async Task Should_Be_Able_To_Get_All_Projects_Matching_The_Query_Filter()
        {
            // 0. Get all first by project name to go sure already existing data not exists
            await Client.AssertGetAsync<GetAllProjectsResponse>($"api/core/v1/projects?name={UniqueName}",
                                                                "NoProjects.json",
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$ProjectName$", UniqueName)]).ConfigureAwait(false);
            
            // 1. Create a new project (1)
            await Client.AssertPostAsync<CreateProjectResponse>("api/core/v1/projects/",
                                                                "CreateProject.json",
                                                                "CreateProject.json",
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$ProjectName$", UniqueName)]).ConfigureAwait(false);
            
            // 2. Create a new project (2)
            await Client.AssertPostAsync<CreateProjectResponse>("api/core/v1/projects/",
                                                                "CreateProject.json",
                                                                "CreateProject.json",
                                                                differenceFunc: IgnoreAutoValues,
                                                                [("$ProjectName$", UniqueName)]).ConfigureAwait(false);

            // 4. Eval that all projects with the specific name was added
            await Client.AssertGetAsync<GetAllProjectsResponse>($"api/core/v1/projects?name={UniqueName}",
                                                                "GetAllProjects.json",
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
