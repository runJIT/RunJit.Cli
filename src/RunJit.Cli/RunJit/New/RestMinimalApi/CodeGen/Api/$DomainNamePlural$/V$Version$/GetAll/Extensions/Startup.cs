namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetAll
{
    internal static class Startup
    {
        internal static void AddGetAll(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGetAllQuery(configuration);
        }

        internal static void UseGetAll(this IEndpointRouteBuilder routeGroupBuilder)
        {
            routeGroupBuilder.MapGetAll();
        }
    }
}
