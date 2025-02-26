using $ProjectName$.Api.Projects;

namespace $ProjectName$.Api
{
    internal static class Startup
    {
        internal static void AddApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddProjects(configuration);
            // Register your api domains here
        }

        internal static void MapApi(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapProjects();
            // Map your endpoints here
        }
    }
}
