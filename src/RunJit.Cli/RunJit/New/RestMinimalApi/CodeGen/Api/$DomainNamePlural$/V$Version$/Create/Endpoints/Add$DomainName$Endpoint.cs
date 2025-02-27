using $ProjectName$.Extensions;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Create
{
    public static class Create$DomainName$Endpoint
    {
        public static void MapCreate$DomainName$(this IEndpointRouteBuilder routeGroupBuilder)
        {
            routeGroupBuilder.MapPost("$DomainNamePluralLower$", async (HttpContext context,
                                                         Create$DomainName$Request create$DomainName$Request,
                                                         Create$DomainName$Command create$DomainName$Command,
                                                         CancellationToken cancellationToken = default)
                                                      =>
                                                  {
                                                      var project = await create$DomainName$Command.ExecuteAsync(context, create$DomainName$Request, cancellationToken).ConfigureAwait(false);

                                                      return new Create$DomainName$Response(project);
                                                  })
                             .Produces<Create$DomainName$Response>()
                             .Produces<ProblemDetails>(401)
                             .Produces<ProblemDetails>(403)
                             .Produces<ProblemDetails>(404)
                             .Produces<ValidationProblemDetails>(422)
                             .Produces<ProblemDetails>(500)
                             .Produces<ProblemDetails>(503)
                             .WithTags("$DomainName$s")
                             .WithName("create$DomainName$V$Version$")
                             .MapToApiVersion(1)
                             .WithDescriptionFromFile("V$Version$.Create.Documentations.Description.txt")
                             .WithSummaryFromFile("V$Version$.Create.Documentations.Summary.txt");
        }
    }
}
