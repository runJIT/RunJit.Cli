using Microsoft.AspNetCore.Http.HttpResults;
using $ProjectName$.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class Delete$DomainName$Endpoint
    {
        internal static void MapDelete$DomainName$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDelete("$DomainNamePluralLower$/{$IdUrlName$:guid}", HandleAsync)
                     .Produces<NoContent>(204)
                     .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                     .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                     .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                     .Produces<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)
                     .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                     .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                     .WithTags("$DomainNamePlural$")
                     .WithName("delete$DomainName$ByIdV$Version$")
                     .MapToApiVersion($Version$)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt");

            static async Task<IResult> HandleAsync(Guid $IdUrlName$,
                                                   Delete$DomainName$Command delete$DomainName$Command,
                                                   CancellationToken cancellationToken = default)
            {
                await delete$DomainName$Command.ExecuteAsync($IdUrlName$, cancellationToken).ConfigureAwait(false);

                return Results.NoContent();
            }
        }
    }
}
