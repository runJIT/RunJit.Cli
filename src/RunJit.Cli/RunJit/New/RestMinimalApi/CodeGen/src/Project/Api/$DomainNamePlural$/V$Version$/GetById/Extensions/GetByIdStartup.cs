namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class GetByIdStartup
    {
        internal static void AddGetById(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGet$DomainName$ByIdQuery(configuration);
        }

        internal static void MapGetById(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet$DomainName$ById();
        }
    }
}
