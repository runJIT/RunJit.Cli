using System.Net.Mime;
using System.Text.Json.Nodes;
using $ProjectName$.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Patch
{
    internal static class PatchEndpoint
    {
        internal static void MapPatch$DomainName$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPatch("$DomainNamePluralLower$/{$IdUrlName$:guid}", HandleAsync)
                     .Accepts<Patch$DomainName$Request>(MediaTypeNames.Application.Json, "Patch request schema for $DomainNameLower$")
                     .Produces<Patch$DomainName$Response>()
                     .Produces<ProblemDetails>(401)
                     .Produces<ProblemDetails>(403)
                     .Produces<ProblemDetails>(404)
                     .Produces<ValidationProblemDetails>(422)
                     .Produces<ProblemDetails>(500)
                     .Produces<ProblemDetails>(503)
                     .WithTags("$DomainNamePlural$")
                     .WithName("patch$DomainName$V1")
                     .MapToApiVersion(1)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt");
                
            static async Task<$DomainName$> HandleAsync(JsonObject patchRequest,
                                                   Patch$DomainName$Command patch$DomainName$Command,
                                                   Guid $IdUrlName$,
                                                   CancellationToken cancellationToken = default)
            {
                var patched$DomainName$ = await patch$DomainName$Command.ExecuteAsync(patchRequest, $IdUrlName$, cancellationToken).ConfigureAwait(false);

                return patched$DomainName$;
            }
        }
    }
}
