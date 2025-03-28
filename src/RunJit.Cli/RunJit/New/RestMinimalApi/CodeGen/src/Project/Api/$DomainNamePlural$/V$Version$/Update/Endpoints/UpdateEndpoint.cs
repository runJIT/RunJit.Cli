using Microsoft.AspNetCore.Mvc;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class UpdateEndpoint
    {
        internal static void MapUpdate$DomainName$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPut("$DomainNamePluralLower$/{$IdUrlName$:guid}", HandleAsync)
                     .Produces<Update$DomainName$Response>()
                     .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                     .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                     .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                     .Produces<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)
                     .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                     .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                     .Produces<string>(StatusCodes.Status504GatewayTimeout) // AWS handled error -> returns HTML
                     .WithTags("$DomainNamePlural$")
                     .WithName("update$DomainName$V$Version$")
                     .MapToApiVersion($Version$)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt")
                     .WithMetadata(new AllowedBodyMetaInfo(typeof(Create$DomainName$Request)));
            
            static async Task<Update$DomainName$Response> HandleAsync(Update$DomainName$Request update$DomainName$Request,
                                                                 Guid $IdUrlName$,
                                                                 Update$DomainName$Command update$DomainName$Command,
                                                                 CancellationToken cancellationToken)
            {
                var updated$DomainName$ = await update$DomainName$Command.ExecuteAsync($IdUrlName$, update$DomainName$Request, cancellationToken).ConfigureAwait(false);

                return new Update$DomainName$Response(updated$DomainName$);
            }
        }
    }
}
