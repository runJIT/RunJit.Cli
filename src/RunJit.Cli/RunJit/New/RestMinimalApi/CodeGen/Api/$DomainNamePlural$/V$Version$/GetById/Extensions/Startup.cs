namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetById
{
    internal static class Startup
    {
        internal static void AddGetById(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGetByIdQuery(configuration);
        }

        internal static void UseGetById(this IEndpointRouteBuilder routeGroupBuilder)
        {
            routeGroupBuilder.MapGetById();
        }
    }
}
