namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.DeleteAll
{
    internal static class Startup
    {
        internal static void AddDeleteAll(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDeleteAll$DomainNamePlural$Command(configuration);
        }

        internal static void MapDeleteAll(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDelete$DomainNamePluralLower$();
        }
    }
}
