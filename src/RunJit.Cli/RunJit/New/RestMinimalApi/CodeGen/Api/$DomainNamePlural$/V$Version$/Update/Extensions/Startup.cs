namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Update
{
    internal static class Startup
    {
        internal static void AddUpdate$DomainName$(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddUpdate$DomainName$RequestTo$DomainName$EntityMapper();
            services.AddUpdate$DomainName$RequestValidator();
            
            services.AddUpdate$DomainName$Command(configuration);
        }

        internal static void MapUpdate$DomainName$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapUpdate$DomainName$();
        }
    }
}
