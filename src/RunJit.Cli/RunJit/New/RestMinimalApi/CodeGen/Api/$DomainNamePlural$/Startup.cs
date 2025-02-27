using $ProjectName$.Api.Projects.V1;

namespace $ProjectName$.Api.Projects
{
    internal static class Startup
    {
        internal static void AddProjects(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            services.AddProjectsV1(configuration);
        }

        internal static void MapProjects(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapProjectsV1();
        }
    }
}
