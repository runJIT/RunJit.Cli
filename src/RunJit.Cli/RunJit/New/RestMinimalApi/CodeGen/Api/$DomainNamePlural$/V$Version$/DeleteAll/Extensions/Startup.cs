namespace $ProjectName$.Api.Projects.V$Version$.DeleteAll
{
    internal static class Startup
    {
        internal static void AddDeleteAllProjects(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDeleteAllProjectsCommand(configuration);
        }

        internal static void MapDeleteAllProjects(this IEndpointRouteBuilder routeGroupBuilder)
        {
            routeGroupBuilder.MapDeleteProject();
        }
    }
}
