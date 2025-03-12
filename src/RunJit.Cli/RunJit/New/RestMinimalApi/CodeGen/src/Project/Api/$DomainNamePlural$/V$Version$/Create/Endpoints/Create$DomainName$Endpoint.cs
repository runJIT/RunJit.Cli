using $ProjectName$.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class Create$DomainName$Endpoint
    {
        internal static void MapCreate$DomainName$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("$DomainNamePluralLower$", HandleAsync)
                     .Produces<Create$DomainName$Response>()
                     .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                     .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                     .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                     .Produces<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)
                     .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                     .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                     .Produces<string>(StatusCodes.Status504GatewayTimeout) // AWS handled error -> returns HTML
                     .WithTags("$DomainNamePlural$")
                     .WithName("create$DomainName$V$Version$")
                     .MapToApiVersion($Version$)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt");
                
            static async Task<Create$DomainName$Response> HandleAsync(Create$DomainName$Request create$DomainName$Request,
                                                                 Create$DomainName$Command create$DomainName$Command,
                                                                 CancellationToken cancellationToken = default)
            {
                var project = await create$DomainName$Command.ExecuteAsync(create$DomainName$Request, cancellationToken).ConfigureAwait(false);

                return new Create$DomainName$Response(project);
            }
        }
    }
}
