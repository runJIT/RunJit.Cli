namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetById
{
    internal static class Startup
    {
        internal static void AddGetById(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGetByIdQuery(configuration);
        }

        internal static void MapGetById(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetById();
        }
    }
}
