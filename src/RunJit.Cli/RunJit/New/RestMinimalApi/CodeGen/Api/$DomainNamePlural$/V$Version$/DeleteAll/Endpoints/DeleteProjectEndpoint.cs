using Microsoft.AspNetCore.Http.HttpResults;
using $ProjectName$.Extensions;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePluralLower$.V$Version$.DeleteAll
{
    public static class DeleteProjectEndpoint
    {
        public static void MapDeleteProject(this IEndpointRouteBuilder routeGroupBuilder)
        {
            routeGroupBuilder.MapDelete("$DomainNamePluralLower$/{$DomainNameLower$Id:guid}", async (
                                                                         Guid $DomainNameLower$Id,
                                                                         DeleteAll$DomainNamePluralLower$Command deleteProjectCommand,
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
                             .WithTags("$DomainNamePluralLower$")
                             .WithName("delete$DomainNamePluralLower$V1")
                             .MapToApiVersion(1)
                             .WithDescriptionFromFile("V1.Delete.Documentations.Description.txt")
                             .WithSummaryFromFile("V1.Delete.Documentations.Summary.txt");
        }
    }
}
