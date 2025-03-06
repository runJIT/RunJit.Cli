using System.Collections.Immutable;
using $ProjectName$.Extensions;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetAll
{
    public static class MapGetAllEndpoint
    {
        public static RouteHandlerBuilder MapGetAll(this IEndpointRouteBuilder endpoints)
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
                
            static async Task<GetAll$DomainNamePlural$Response> HandleAsync(GetAllQuery getAllQuery,
                                                                            [FromQuery] string $QueryPropertyNameLower$ = "")
            {
                var $DomainNamePluralLower$ = await getAllQuery.ExecuteAsync($QueryPropertyNameLower$).ConfigureAwait(false);

                return new GetAll$DomainNamePlural$Response($DomainNamePluralLower$);
            }
        }
    }
}
