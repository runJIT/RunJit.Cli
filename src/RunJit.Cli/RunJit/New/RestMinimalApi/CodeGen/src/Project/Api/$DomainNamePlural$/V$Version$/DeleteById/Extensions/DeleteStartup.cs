namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class DeleteStartup
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
