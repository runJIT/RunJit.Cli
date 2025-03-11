using Microsoft.AspNetCore.Mvc;
using $ProjectName$.Extensions;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class MapGetAll$DomainNamePluralLower$Endpoint
    {
        internal static RouteHandlerBuilder MapGetAll$DomainNamePlural$(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapGet("$DomainNamePluralLower$", HandleAsync)
                            .Produces<GetAll$DomainNamePlural$Response>()
                            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                            .Produces<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)
                            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                            .WithTags("$DomainNamePlural$")
                            .WithName("getAll$DomainNamePlural$V$Version$")
                            .MapToApiVersion($Version$)
                            .WithDescriptionFromFile("Description.txt")
                            .WithSummaryFromFile("Summary.txt");
                
            static async Task<GetAll$DomainNamePlural$Response> HandleAsync(GetAll$DomainNamePlural$Query getAll$DomainNamePlural$Query,
                                                                            [FromQuery] string $QueryPropertyNameLower$ = "",
                                                                            CancellationToken cancellationToken = default)
            {
                var $DomainNamePluralLower$ = await getAll$DomainNamePlural$Query.ExecuteAsync($QueryPropertyNameLower$, cancellationToken).ConfigureAwait(false);

                return new GetAll$DomainNamePlural$Response($DomainNamePluralLower$);
            }
        }
    }
}
