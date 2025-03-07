using $ProjectName$.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Create
{
    internal static class CreateProjectEndpoint
    {
        internal static void MapCreateProject(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("$DomainNamePluralLower$", HandleAsync)
                     .Produces<CreateProjectResponse>()
                     .Produces<ProblemDetails>(401)
                     .Produces<ProblemDetails>(403)
                     .Produces<ProblemDetails>(404)
                     .Produces<ValidationProblemDetails>(422)
                     .Produces<ProblemDetails>(500)
                     .Produces<ProblemDetails>(503)
                     .WithTags("$DomainNamePlural$")
                     .WithName("createProjectV$Version$")
                     .MapToApiVersion($Version$)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt");
                
            static async Task<CreateProjectResponse> HandleAsync(CreateProjectRequest createProjectRequest,
                                                                 CreateProjectCommand createProjectCommand,
                                                                 CancellationToken cancellationToken = default)
            {
                var project = await createProjectCommand.ExecuteAsync(createProjectRequest, cancellationToken).ConfigureAwait(false);

                return new CreateProjectResponse(project);
            }
        }
    }
}
