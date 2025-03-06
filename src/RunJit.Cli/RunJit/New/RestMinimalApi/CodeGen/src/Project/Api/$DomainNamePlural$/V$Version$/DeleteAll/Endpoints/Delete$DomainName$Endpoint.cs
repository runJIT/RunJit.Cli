using Microsoft.AspNetCore.Http.HttpResults;
using $ProjectName$.Extensions;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.DeleteAll
{
    internal static class DeleteProjectEndpoint
    {
        internal static void MapDeleteProject(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDelete("$DomainNamePluralLower$", HandleAsync)
                     .Produces<NoContent>(204)
                     .Produces<ProblemDetails>(401)
                     .Produces<ProblemDetails>(403)
                     .Produces<ProblemDetails>(404)
                     .Produces<ValidationProblemDetails>(422)
                     .Produces<ProblemDetails>(500)
                     .Produces<ProblemDetails>(503)
                     .WithTags("$DomainNamePluralLower$")
                     .WithName("deleteAll$DomainNamePlural$V$Version$")
                     .MapToApiVersion($Version$)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt");

            static async Task<IResult> HandleAsync(DeleteAll$DomainNamePlural$Command deleteProjectCommand,
                                                   [FromQuery] string $QueryPropertyNameLower$ = "",
                                                   CancellationToken cancellationToken = default)
            {
                await deleteProjectCommand.ExecuteAsync($QueryPropertyNameLower$, cancellationToken).ConfigureAwait(false);

                return Results.NoContent();
            }
        }
    }
}
