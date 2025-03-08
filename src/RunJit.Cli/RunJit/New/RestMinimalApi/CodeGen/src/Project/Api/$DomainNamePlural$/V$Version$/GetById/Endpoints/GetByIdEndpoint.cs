using $ProjectName$.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static RouteHandlerBuilder MapGet$DomainName$ById(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("$DomainNamePluralLower$/{$IdUrlName$:guid}", Handler)
                        .Produces<Get$DomainName$ByIdResponse>()
                        .Produces<ProblemDetails>(401)
                        .Produces<ProblemDetails>(403)
                        .Produces<ProblemDetails>(404)
                        .Produces<ValidationProblemDetails>(422)
                        .Produces<ProblemDetails>(500)
                        .Produces<ProblemDetails>(503)
                        .WithTags("$DomainNamePlural$")
                        .WithName("get$DomainName$ByIdV$Version$")
                        .MapToApiVersion(1)
                        .WithDescriptionFromFile("Description.txt")
                        .WithSummaryFromFile("Summary.txt");

        static async Task<Get$DomainName$ByIdResponse> Handler(Guid $DomainNameLower$Id,
                                                          Get$DomainName$ByIdQuery get$DomainName$ByIdQuery,
                                                          CancellationToken cancellationToken = default)
        {
            var $DomainNameLower$ = await get$DomainName$ByIdQuery.ExecuteAsync($DomainNameLower$Id, cancellationToken).ConfigureAwait(false);

            return new Get$DomainName$ByIdResponse($DomainNameLower$);
        }
    }
}
