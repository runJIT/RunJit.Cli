using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;
using Siemens.AspNet.MinimalApi.Sdk.Contracts.Endpoints;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class AddMapGetAll$DomainNamePluralLower$EndpointExtension
    {
        internal static void AddMapGetAll$DomainNamePluralLower$Endpoint(this IServiceCollection services,
                                                                         IConfiguration configuration)
        {
            services.AddGetAll$DomainNamePlural$Query(configuration);

            services.AddSingletonIfNotExists<IEndpoint, MapGetAll$DomainNamePluralLower$Endpoint>();
        }
    }

    internal sealed class MapGetAll$DomainNamePluralLower$Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("$DomainNamePluralLower$", HandleAsync)
                            .Produces<GetAll$DomainNamePlural$Response>()
                            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                            .Produces<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)
                            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                            .Produces<string>(StatusCodes.Status504GatewayTimeout) // AWS handled error -> returns HTML
                            .WithTags("$DomainNamePlural$")
                            .WithName("getAll$DomainNamePlural$V$Version$")
                            .MapToApiVersion($Version$)
                            .WithDescriptionFromFile("Description.txt")
                            .WithSummaryFromFile("Summary.txt")
                            .WithMetadata(new AllowedQueryParameterMetaInfo("$QueryPropertyNameLower$"));

            static async Task<GetAll$DomainNamePlural$Response> HandleAsync(GetAll$DomainNamePlural$Query getAll$DomainNamePlural$Query,
                                                                            [FromQuery] string? $QueryPropertyNameLower$ = null,
                                                                            CancellationToken cancellationToken = default)
            {
                var request = new GetAll$DomainNamePlural$Request
                {
                    $QueryPropertyName$ = $QueryPropertyNameLower$
                };

                var $DomainNamePluralLower$ = await getAll$DomainNamePlural$Query.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);

                return new GetAll$DomainNamePlural$Response($DomainNamePluralLower$);
            }
        }
    }
}
