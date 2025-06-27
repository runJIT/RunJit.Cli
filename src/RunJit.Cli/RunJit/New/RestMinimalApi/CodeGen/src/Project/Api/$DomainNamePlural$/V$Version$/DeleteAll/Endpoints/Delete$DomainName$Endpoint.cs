using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class Delete$DomainNamePlural$Endpoint
    {
        internal static void MapDelete$DomainNamePlural$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDelete("$DomainNamePluralLower$", HandleAsync)
                     .Produces(StatusCodes.Status204NoContent)
                     .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
                     .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                     .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                     .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                     .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                     .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                     .Produces<string>(StatusCodes.Status504GatewayTimeout) // AWS handled error -> returns HTML
                     .WithTags("$DomainNamePlural$")
                     .WithName("deleteAll$DomainNamePlural$V$Version$")
                     .MapToApiVersion($Version$)
                     .WithDescriptionFromFile("Description.txt")
                     .WithSummaryFromFile("Summary.txt")
                     .WithMetadata(new AllowedQueryParameterMetaInfo("$QueryPropertyNameLower$"));

            static async Task<IResult> HandleAsync(DeleteAll$DomainNamePlural$Command deleteAll$DomainNamePlural$Command,
                                                   [FromQuery] string? $QueryPropertyNameLower$ = null,
                                                   CancellationToken cancellationToken = default)
            {
                var request = new DeleteAll$DomainNamePlural$Request
                {
                    $QueryPropertyName$ = $QueryPropertyNameLower$
                };
                
                await deleteAll$DomainNamePlural$Command.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);

                return Results.NoContent();
            }
        }
    }
}
