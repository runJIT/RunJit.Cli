using Microsoft.AspNetCore.Http.HttpResults;
using $ProjectName$.Extensions;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Delete
{
    public static class Delete$DomainName$Endpoint
    {
        public static void MapDelete$DomainName$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDelete("$DomainNamePluralLower$/{$DomainNameLower$Id:guid}", async (
                                                                         Guid $DomainNameLower$Id,
                                                                         Delete$DomainName$Command delete$DomainName$Command,
                                                                         CancellationToken cancellationToken = default
                                                                     ) =>
                                                                     {
                                                                         await delete$DomainName$Command.ExecuteAsync($DomainNameLower$Id, cancellationToken).ConfigureAwait(false);
                                                                         return Results.NoContent();
                                                                     })
                             .Produces<NoContent>(204)
                             .Produces<ProblemDetails>(401)
                             .Produces<ProblemDetails>(403)
                             .Produces<ProblemDetails>(404)
                             .Produces<ValidationProblemDetails>(422)
                             .Produces<ProblemDetails>(500)
                             .Produces<ProblemDetails>(503)
                             .WithTags("$DomainNamePlural$")
                             .WithName("delete$DomainNamePlural$V$Version$")
                             .MapToApiVersion(1)
                             .WithDescriptionFromFile("V$Version$.Delete.Documentations.Description.txt")
                             .WithSummaryFromFile("V$Version$.Delete.Documentations.Summary.txt");
        }
    }
}
