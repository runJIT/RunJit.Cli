using $ProjectName$.Extensions;
using Microsoft.AspNetCore.Mvc;

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
                            .WithTags("$DomainNamePlural$")
                            .WithName("get$DomainName$ByIdV$Version$")
                            .MapToApiVersion(1)
                            .WithDescriptionFromFile("Description.txt")
                            .WithSummaryFromFile("Summary.txt");

            static async Task<Get$DomainName$ByIdResponse> HandleAsync(Guid $DomainNameLower$Id,
                                                                       Get$DomainName$ByIdQuery get$DomainName$ByIdQuery,
                                                                       CancellationToken cancellationToken = default)
            {
                var $DomainNameLower$ = await get$DomainName$ByIdQuery.ExecuteAsync($DomainNameLower$Id, cancellationToken).ConfigureAwait(false);

                return new Get$DomainName$ByIdResponse($DomainNameLower$);
            }
        }
    }
}
