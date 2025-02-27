namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.DeleteAll
{
    internal static class Startup
    {
        internal static void AddDeleteAll$DomainNamePlural$(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDeleteAll$DomainNamePlural$Command(configuration);
        }

        internal static void MapDeleteAll$DomainNamePlural$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDeleteProject();
        }
    }
}
