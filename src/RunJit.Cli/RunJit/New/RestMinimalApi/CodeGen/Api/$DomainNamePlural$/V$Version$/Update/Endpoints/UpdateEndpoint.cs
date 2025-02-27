using $ProjectName$.Api.$DomainNamePlural$.V1.Update;
using $ProjectName$.Extensions;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Update
{
    public static class UpdateEndpoint
    {
        public static void MapUpdate$DomainName$(this IEndpointRouteBuilder routeGroupBuilder)
        {
            routeGroupBuilder.MapPut("$DomainNamePluralLower$/{$DomainNameLower$Id}", async (HttpContext httpContext,
                                                                    Update$DomainName$Request update$DomainName$Request,
                                                                    Guid $DomainNameLower$Id,
                                                                    Update$DomainName$Command update$DomainName$Command,
                                                                    CancellationToken cancellationToken = default
                                                             ) =>
                                                             {
                                                                 var updated$DomainName$ = await update$DomainName$Command.ExecuteAsync(httpContext, $DomainNameLower$Id, update$DomainName$Request, cancellationToken).ConfigureAwait(false);

                                                                 return new Update$DomainName$Response(updated$DomainName$);
                                                             }

                                    )
                             .Produces<Update$DomainName$Response>(200)
                             .Produces<ProblemDetails>(401)
                             .Produces<ProblemDetails>(403)
                             .Produces<ProblemDetails>(404)
                             .Produces<ValidationProblemDetails>(422)
                             .Produces<ProblemDetails>(500)
                             .Produces<ProblemDetails>(503)
                             .WithTags("$DomainNamePlural$")
                             .WithName("update$DomainName$V$Version$")
                             .MapToApiVersion(1)
                             .WithDescriptionFromFile("V$Version$.Update.Documentations.Description.txt")
                             .WithSummaryFromFile("V$Version$.Update.Documentations.Summary.txt");
        }
    }
}
