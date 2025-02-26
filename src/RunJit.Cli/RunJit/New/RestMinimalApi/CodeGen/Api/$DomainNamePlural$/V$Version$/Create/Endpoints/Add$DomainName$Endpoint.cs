using $ProjectName$.Extensions;
using Siemens.AspNet.ErrorHandling.Contracts;
using $ProjectName$.Api.$DomainNamePlural$.V$Version$.Create;

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


// features/rework
// // Minimal APIs implementieren
// // ACL
// // ErrorCodes (400,401,403,500,503)... definieren zentral und nutzen
// // container in lambda
// // Websocket for updates when someone created a new resource
// // ID gegen GUID Tauscher (dafür gibt es was in dotnet, der aus nummern GUIDS machen kann)
// // Caching von Jakob optimieren + einbauen
// // Client Gen für Dotnet + TS (Angular)
// // Anpassung der Appsettings (jedes Setting muss auch 
// // X-Ray + OpenTelemetry + Logging
// // Alle SQLs in SQL Files auslagern
// // Migrations schreiben
// // 

// Testing + Quality
// // Tests auf Client Ebene
// // Benchmarks gegen die implementierung als auch zum Vergleich mit alten APIs
// // Code Rules auf Basis der neuen Implementierung
// // Validatoren einbauen pro Klasse
// // Loadtests with much more data
// // Tests running on test tenant
// // 


/*
    .Produces(200, typeof(GetAllProjectsResponse)) // normal response
    .Produces(201, typeof(Results.Created)) // for fire&forget
    .Produces(204, typeof(Results.NoContent)) // for delete operations
    .Produces(400, typeof(PulseProblemDetails)) // structure of the reuqest is not correct
    .Produces(403, typeof(PulseProblemDetails)) // forbidden (includes 401, 404 + 403)
    .Produces(422, typeof(PulseProblemValidationDetails)) // object was correct, but validation error occurs
    .Produces(500, typeof(PulseProblemDetails)) // general server error
    .Produces(503, typeof(PulseProblemDetails)) // for feature toggeling
*/
