using Microsoft.AspNetCore.Mvc;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class MapGet$DomainName$ByIdEndpoint
    {
        internal static RouteHandlerBuilder MapGet$DomainName$ById(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapGet("$DomainNamePluralLower$/{$IdUrlName$:guid}", HandleAsync)
                            .Produces<Get$DomainName$ByIdResponse>()
                            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                            .Produces<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)
                            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                            .Produces<string>(StatusCodes.Status504GatewayTimeout) // AWS handled error -> returns HTML
                            .WithTags("$DomainNamePlural$")
                            .WithName("get$DomainName$ByIdV$Version$")
                            .MapToApiVersion(1)
                            .WithDescriptionFromFile("Description.txt")
                            .WithSummaryFromFile("Summary.txt");

            static async Task<Get$DomainName$ByIdResponse> HandleAsync(Guid $IdUrlName$,
                                                                       Get$DomainName$ByIdQuery get$DomainName$ByIdQuery,
                                                                       CancellationToken cancellationToken = default)
            {
                var request = new Get$DomainName$ByIdRequest
                {
                    $IdPropertyName$ = $IdUrlName$
                };
                
                var $DomainNameLower$ = await get$DomainName$ByIdQuery.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);

                return new Get$DomainName$ByIdResponse($DomainNameLower$);
            }
        }
    }
}
