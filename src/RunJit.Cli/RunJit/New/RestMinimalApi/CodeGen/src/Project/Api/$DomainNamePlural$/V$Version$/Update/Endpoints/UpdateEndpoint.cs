using $ProjectName$.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class UpdateEndpoint
    {
        internal static void MapUpdate$DomainName$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPut("$DomainNamePluralLower$/{$IdUrlName$:guid}", HandleAsync)
                     .Produces<Update$DomainName$Response>()
                     .Produces<ProblemDetails>(401)
                     .Produces<ProblemDetails>(403)
                     .Produces<ProblemDetails>(404)
                     .Produces<ValidationProblemDetails>(422)
                     .Produces<ProblemDetails>(500)
                     .Produces<ProblemDetails>(503)
                     .WithTags("$DomainNamePlural$")
                     .WithName("update$DomainName$V$Version$")
                     .MapToApiVersion($Version$)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt");
            
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
