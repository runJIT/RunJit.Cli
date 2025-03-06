namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Delete
{
    internal static class Startup
    {
        internal static void AddDeleteById(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDelete$DomainName$Command(configuration);
        }

        internal static void MapDeleteById(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDelete$DomainName$();
        }
    }
}
