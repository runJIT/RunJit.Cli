using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using $ProjectName$.Extensions;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class Delete$DomainNamePluralLower$Endpoint
    {
        internal static void MapDelete$DomainNamePluralLower$(this IEndpointRouteBuilder endpoints)
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
