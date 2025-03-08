namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class UpdateStartup
    {
        internal static void AddUpdate(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddUpdate$DomainName$Command(configuration);
        }

        internal static void MapUpdate(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapUpdate$DomainName$();
        }
    }
}
