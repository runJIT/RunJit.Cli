using Microsoft.AspNetCore.Http.HttpResults;
using $ProjectName$.Extensions;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Delete
{
public static class DeleteProjectEndpoint
{
    public static void MapDeleteProject(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("$DomainNamePluralLower$/{$IdUrlName$:guid}", HandleAsync)
                 .Produces<NoContent>(204)
                 .Produces<ProblemDetails>(401)
                 .Produces<ProblemDetails>(403)
                 .Produces<ProblemDetails>(404)
                 .Produces<ValidationProblemDetails>(422)
                 .Produces<ProblemDetails>(500)
                 .Produces<ProblemDetails>(503)
                 .WithTags("$DomainNamePlural$")
                 .WithName("delete$DomainNamePlural$ByIdV$Version$")
                 .MapToApiVersion(1)
                 .WithDescriptionFromFile("Description.txt")
                 .WithSummaryFromFile("Summary.txt");

        static async Task<IResult> HandleAsync(Guid $IdUrlName$,
                                               DeleteProjectCommand deleteProjectCommand,
                                               CancellationToken cancellationToken = default)
        {
            await deleteProjectCommand.ExecuteAsync($IdUrlName$, cancellationToken).ConfigureAwait(false);

            return Results.NoContent();
        }
    }
}
}

}
