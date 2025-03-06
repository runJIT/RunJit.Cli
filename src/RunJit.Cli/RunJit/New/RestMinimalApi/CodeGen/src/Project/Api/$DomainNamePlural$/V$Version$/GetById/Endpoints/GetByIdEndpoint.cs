using $ProjectName$.Extensions;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Get$DomainName$ById
{
    public sealed record Get$DomainName$ByIdResponse($DomainName$ $DomainName$);

    public static class MapGet$DomainName$ByIdEndpoint
    {
        public static RouteHandlerBuilder MapGet$DomainName$ById(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapGet("$DomainNamePluralLower$/{$IdUrlName$:guid}",
                                            async (Guid $DomainNameLower$Id,
                                                   Get$DomainName$ByIdQuery get$DomainName$ByIdQuery,
                                                   CancellationToken cancellationToken = default) =>
                                            {
                                                var $DomainNameLower$ = await get$DomainName$ByIdQuery.ExecuteAsync($IdUrlName$, cancellationToken).ConfigureAwait(false); 
                                                return new Get$DomainName$ByIdResponse($DomainNameLower$);
                                            })

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
        }
    }
}
