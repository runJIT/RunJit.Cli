using $ProjectName$.Api.$DomainNamePlural$.V$Version$._Shared_;
using $ProjectName$.Extensions;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetById
{
    public sealed record Get$DomainName$ByIdResponse($DomainName$ $DomainName$);

    public static class MapGetByIdEndpoint
    {
        public static RouteHandlerBuilder MapGetById(this IEndpointRouteBuilder routeGroupBuilder)
        {
            return routeGroupBuilder.MapGet("$DomainNamePluralLower$/{projectId:guid}",
                                            async (Guid projectId,
                                                   GetByIdQuery getByIdQuery,
                                                   CancellationToken cancellationToken = default) =>
                                            {
                                                var $DomainNameLower$ = await getByIdQuery.ExecuteAsync(projectId, cancellationToken).ConfigureAwait(false); 
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
                                    .WithDescriptionFromFile("V$Version$.GetById.Documentations.Description.txt")
                                    .WithSummaryFromFile("V$Version$.GetById.Documentations.Summary.txt");
        }
    }
}
