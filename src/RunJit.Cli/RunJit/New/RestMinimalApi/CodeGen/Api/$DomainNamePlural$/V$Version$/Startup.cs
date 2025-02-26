using $ProjectName$.Api.Projects.V$Version$.Create;
using $ProjectName$.Api.Projects.V$Version$.Delete;
using $ProjectName$.Api.Projects.V$Version$.DeleteAll;
using $ProjectName$.Api.Projects.V$Version$.GetAll;
using $ProjectName$.Api.Projects.V$Version$.GetById;
using $ProjectName$.Api.Projects.V$Version$.Patch;
using $ProjectName$.Api.Projects.V$Version$.Update;

namespace $ProjectName$.Api.Projects.V1
{
    internal static class Startup
    {
        internal static void AddProjectsV1(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddCreate(configuration);
            serviceCollection.AddDeleteAll(configuration);
            serviceCollection.AddDeleteById(configuration);
            serviceCollection.AddGetAll(configuration);
            serviceCollection.AddGetById(configuration);
            serviceCollection.AddPatchProject(configuration);
            serviceCollection.AddUpdateProject(configuration);
        }

        internal static void MapProjectsV1(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapCreate();
            endpointRouteBuilder.MapDeleteAll();
            endpointRouteBuilder.MapDeleteById();
            endpointRouteBuilder.MapGetAll();
            endpointRouteBuilder.MapGetById();
            endpointRouteBuilder.MapPatch();
            endpointRouteBuilder.MapUpdate();
        }
    }
}
