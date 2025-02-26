using Microsoft.AspNetCore.Http.HttpResults;
using $ProjectName$.Extensions;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.Projects.V$Version$.Delete
{
    public static class DeleteProjectEndpoint
    {
        public static void MapDeleteProject(this IEndpointRouteBuilder routeGroupBuilder)
        {
            routeGroupBuilder.MapDelete("projects/{projectId:guid}", async (
                                                                         Guid projectId,
                                                                         DeleteProjectCommand deleteProjectCommand,
                                                                         CancellationToken cancellationToken = default
                                                                     ) =>
                                                                     {
                                                                         await deleteProjectCommand.ExecuteAsync(projectId, cancellationToken).ConfigureAwait(false);
                                                                         return Results.NoContent();
                                                                     })
                             .Produces<NoContent>(204)
                             .Produces<ProblemDetails>(401)
                             .Produces<ProblemDetails>(403)
                             .Produces<ProblemDetails>(404)
                             .Produces<ValidationProblemDetails>(422)
                             .Produces<ProblemDetails>(500)
                             .Produces<ProblemDetails>(503)
                             .WithTags("Projects")
                             .WithName("deleteProjectsV$Version$")
                             .MapToApiVersion(1)
                             .WithDescriptionFromFile("V$Version$.Delete.Documentations.Description.txt")
                             .WithSummaryFromFile("V$Version$.Delete.Documentations.Summary.txt");
        }
    }
}
