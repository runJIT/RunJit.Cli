using Microsoft.AspNetCore.Mvc;
using $ProjectName$.Extensions;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetAll
{
    internal static class MapGetAll$DomainNamePluralLower$Endpoint
    {
        internal static RouteHandlerBuilder MapGetAll$DomainNamePlural$(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapGet("$DomainNamePluralLower$", HandleAsync)
                            .Produces<GetAll$DomainNamePlural$Response>()
                            .Produces<ProblemDetails>(401)
                            .Produces<ProblemDetails>(403)
                            .Produces<ProblemDetails>(404)
                            .Produces<ValidationProblemDetails>(422)
                            .Produces<ProblemDetails>(500)
                            .Produces<ProblemDetails>(503)
                            .WithTags("$DomainNamePlural$")
                            .WithName("getAll$DomainNamePlural$V$Version$")
                            .MapToApiVersion(1)
                            .WithDescriptionFromFile("Description.txt")
                            .WithSummaryFromFile("Summary.txt");
                
            static async Task<GetAll$DomainNamePlural$Response> HandleAsync(GetAll$DomainNamePlural$Query getAll$DomainNamePlural$Query,
                                                                            [FromQuery] string $QueryPropertyNameLower$ = "")
            {
                var $DomainNamePluralLower$ = await getAll$DomainNamePlural$Query.ExecuteAsync($QueryPropertyNameLower$).ConfigureAwait(false);

                return new GetAll$DomainNamePlural$Response($DomainNamePluralLower$);
            }
        }
    }
}
