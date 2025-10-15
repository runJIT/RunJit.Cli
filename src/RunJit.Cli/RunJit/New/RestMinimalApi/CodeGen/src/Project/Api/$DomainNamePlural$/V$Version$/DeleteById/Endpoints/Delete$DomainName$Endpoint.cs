using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class AddDelete$DomainName$EndpointExtension
    {
        internal static void AddDelete$DomainName$Endpoint(this IServiceCollection services,
                                                           IConfiguration configuration)
        {
            services.AddDelete$DomainName$Command(configuration);

            services.AddSingletonIfNotExists<IEndpoint, Delete$DomainName$Endpoint>();
        }
    }

    internal sealed class Delete$DomainName$Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDelete("$DomainNamePluralLower$/{$IdUrlName$:guid}", HandleAsync)
                     .Produces(StatusCodes.Status204NoContent)
                     .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
                     .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                     .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                     .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                     .Produces<ValidationProblemDetailsExtended>(StatusCodes.Status422UnprocessableEntity)
                     .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                     .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                     .Produces<string>(StatusCodes.Status504GatewayTimeout) // AWS handled error -> returns HTML
                     .WithTags("$DomainNamePlural$")
                     .WithName("delete$DomainName$ByIdV$Version$")
                     .MapToApiVersion($Version$)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt");

            static async Task<IResult> HandleAsync(Guid $IdUrlName$,
                                                   Delete$DomainName$Command delete$DomainName$Command,
                                                   CancellationToken cancellationToken = default)
            {
                var request = new Delete$DomainName$ByIdRequest
                {
                    $IdPropertyName$ = $IdUrlName$
                };

                await delete$DomainName$Command.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);

                return Results.NoContent();
            }
        }
    }
}
