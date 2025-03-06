using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using Sdc.Core.Api.Projects.V1;
using Sdc.Core.Api.Projects.V1.Create;
using Sdc.Core.Api.Projects.V1.GetById;
using Sdc.Core.Api.Projects.V1.Patch;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace Sdc.Core.Test.Api.Health
{
    [TestClass]
    [TestCategory("Projects")]
    [TestCategory("Projects V1")]
    public class Patch_Project_Test : ApiTestBase
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
            return Client.AssertPatchAsUnauthorizedAsync("api/core/v1/projects");
        }

        [TestMethod]
        public Task Should_Not_Be_Able_To_Patch_If_Id_Is_Not_A_Guid()
        {
            return Client.AssertPatchAsync<ProblemDetails>("api/core/v1/projects/not-a-guid",
                                                           "InvalidId.json");
        }

        [TestMethod]
        public async Task Should_Be_Able_To_Patch_A_Project_With_Valid_Data()
        {
            // 1. Create a new project
            var createProjectResponse = await Client.AssertPostAsync<CreateProjectResponse>("api/core/v1/projects/",
                                                                                            "CreateProject.json",
                                                                                            "CreateProject.json",
                                                                                            IgnoreAutoValues,
                                                                                            [("$ProjectName$", UniqueName)]).ConfigureAwait(false);

            // 2. Get the currently created project
            await Client.AssertGetAsync<GetProjectByIdResponse>($"api/core/v1/projects/{createProjectResponse.Project.Id}",
                                                                "CreateProject.json",
                                                                IgnoreAutoValues,
                                                                [("$ProjectName$", UniqueName)]).ConfigureAwait(false);

            // 3. Patch the existing project
            await Client.AssertPatchAsync<PatchProjectResponse>($"api/core/v1/projects/{createProjectResponse.Project.Id}",
                                                                "PatchProject.json",
                                                                "PatchProject.json",
                                                                IgnoreAutoValues,
                                                                [("$ProjectName$", UniqueName)]).ConfigureAwait(false);
        }

        [DataTestMethod]
        [DataRow("InvalidPatch.json", "InvalidPatch.json")]
        public async Task Should_Return_Bad_Request_If_Data_Not_Matching(string request,
                                                                         string response)
        {
            // 1. Create a new project
            var createProjectResponse = await Client.AssertPostAsync<CreateProjectResponse>("api/core/v1/projects/",
                                                                                            "CreateProject.json",
                                                                                            "CreateProject.json",
                                                                                            IgnoreAutoValues,
                                                                                            [("$ProjectName$", UniqueName)],
                                                                                            writeResponse: true).ConfigureAwait(false);

            // 2. Try to patch project with properties which not exists.
            await Client.AssertPatchAsErrorAsync<ValidationProblemDetails>($"api/core/v1/projects/{createProjectResponse.Project.Id}",
                                                                           request,
                                                                           response,
                                                                           IgnoreAutoValues,
                                                                           [("$ProjectName$", UniqueName),
                                                                            ("$ProjectId$", createProjectResponse.Project.Id)]).ConfigureAwait(false);
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
