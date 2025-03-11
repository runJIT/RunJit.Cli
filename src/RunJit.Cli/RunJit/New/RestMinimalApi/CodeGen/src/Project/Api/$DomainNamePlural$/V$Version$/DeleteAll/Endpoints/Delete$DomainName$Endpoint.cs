using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using $ProjectName$.Extensions;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class Delete$DomainNamePlural$Endpoint
    {
        internal static void MapDelete$DomainNamePlural$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDelete("$DomainNamePluralLower$", HandleAsync)
                     .Produces<NoContent>(204)
                     .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                     .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                     .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                     .Produces<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)
                     .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                     .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                     .WithTags("$DomainNamePlural$")
                     .WithName("deleteAll$DomainNamePlural$V$Version$")
                     .MapToApiVersion($Version$)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt");

            static async Task<IResult> HandleAsync(DeleteAll$DomainNamePlural$Command delete$DomainName$Command,
                                                   [FromQuery] string $QueryPropertyNameLower$ = "",
                                                   CancellationToken cancellationToken = default)
            {
                await delete$DomainName$Command.ExecuteAsync($QueryPropertyNameLower$, cancellationToken).ConfigureAwait(false);

                return Results.NoContent();
            }
        }
    }
}
