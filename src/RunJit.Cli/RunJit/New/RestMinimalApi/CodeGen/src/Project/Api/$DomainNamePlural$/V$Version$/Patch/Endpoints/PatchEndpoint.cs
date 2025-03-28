using System.Net.Mime;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class PatchEndpoint
    {
        internal static void MapPatch$DomainName$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPatch("$DomainNamePluralLower$/{$IdUrlName$:guid}", HandleAsync)
                     .Accepts<Patch$DomainName$Request>(MediaTypeNames.Application.Json, "Patch request schema for $DomainNameLower$")
                     .Produces<Patch$DomainName$Response>()
                     .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                     .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                     .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                     .Produces<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)
                     .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                     .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                     .Produces<string>(StatusCodes.Status504GatewayTimeout) // AWS handled error -> returns HTML
                     .WithTags("$DomainNamePlural$")
                     .WithName("patch$DomainName$V$Version$")
                     .MapToApiVersion($Version$)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt")
                     .WithMetadata(new AllowedBodyMetaInfo(typeof(Create$DomainName$Request)));
                
            static async Task<Patch$DomainName$Response> HandleAsync(JsonObject patchRequest,
                                                                     Patch$DomainName$Command patch$DomainName$Command,
                                                                     Guid $IdUrlName$,
                                                                     CancellationToken cancellationToken = default)
            {
                var patched$DomainName$ = await patch$DomainName$Command.ExecuteAsync(patchRequest, $IdUrlName$, cancellationToken).ConfigureAwait(false);

                return new Patch$DomainName$Response(patched$DomainName$);
            }
        }
    }
}
