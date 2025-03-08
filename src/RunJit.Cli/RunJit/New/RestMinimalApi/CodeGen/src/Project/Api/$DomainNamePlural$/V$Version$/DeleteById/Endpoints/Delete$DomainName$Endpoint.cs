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
                     .Produces<ProblemDetails>(401)
                     .Produces<ProblemDetails>(403)
                     .Produces<ProblemDetails>(404)
                     .Produces<ValidationProblemDetails>(422)
                     .Produces<ProblemDetails>(500)
                     .Produces<ProblemDetails>(503)
                     .WithTags("$DomainNamePlural$")
                     .WithName("delete$DomainNamePlural$ByIdV$Version$")
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
