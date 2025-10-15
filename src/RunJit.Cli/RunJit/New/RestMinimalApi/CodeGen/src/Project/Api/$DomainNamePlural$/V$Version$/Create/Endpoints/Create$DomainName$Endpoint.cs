using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;
using Siemens.AspNet.MinimalApi.Sdk.Contracts.Endpoints;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{

    internal static class AddCreate$DomainName$EndpointExtension
    {
        internal static void AddCreate$DomainName$Endpoint(this IServiceCollection services,
                                                           IConfiguration configuration)
        {
            services.AddCreate$DomainName$Command(configuration);

            services.AddSingletonIfNotExists<IEndpoint, Create$DomainName$Endpoint>();
        }
    }

    internal sealed class Create$DomainName$Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("$DomainNamePluralLower$", HandleAsync)
                     .Produces<Create$DomainName$Response>()
                     .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
                     .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                     .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                     .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                     .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
                     .Produces<ValidationProblemDetailsExtended>(StatusCodes.Status422UnprocessableEntity)
                     .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                     .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                     .Produces<string>(StatusCodes.Status504GatewayTimeout) // AWS handled error -> returns HTML
                     .WithTags("$DomainNamePlural$")
                     .WithName("create$DomainName$V$Version$")
                     .MapToApiVersion($Version$)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt")
                     .WithMetadata(new AllowedBodyMetaInfo(typeof(Create$DomainName$Request)));

            static async Task<Create$DomainName$Response> HandleAsync(Create$DomainName$Request create$DomainName$Request,
                                                                 Create$DomainName$Command create$DomainName$Command,
                                                                 CancellationToken cancellationToken = default)
            {
                var project = await create$DomainName$Command.ExecuteAsync(create$DomainName$Request, cancellationToken).ConfigureAwait(false);

                return new Create$DomainName$Response(project);
            }
        }
    }
}
