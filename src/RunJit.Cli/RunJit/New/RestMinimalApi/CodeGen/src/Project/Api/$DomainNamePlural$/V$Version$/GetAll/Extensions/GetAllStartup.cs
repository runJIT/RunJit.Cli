namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class GetAllStartup
    {
        internal static void AddGetAll(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGetAll$DomainNamePlural$Query(configuration);
        }

        internal static void MapGetAll(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetAll$DomainNamePlural$();
        }
    }
}
