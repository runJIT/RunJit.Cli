namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Create
{
    internal static class Startup
    {
        internal static void AddCreate$DomainName$(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCreate$DomainName$Command(configuration);
        }

        internal static void MapCreate$DomainName$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapCreate$DomainName$();
        }
    }
}
