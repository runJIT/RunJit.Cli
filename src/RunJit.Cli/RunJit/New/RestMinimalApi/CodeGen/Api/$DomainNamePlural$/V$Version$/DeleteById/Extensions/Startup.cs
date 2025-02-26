namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Delete
{
    internal static class Startup
    {
        internal static void AddDelete$DomainName$ById(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDelete$DomainName$Command(configuration);
        }

        internal static void MapDelete$DomainName$ById(this IEndpointRouteBuilder routeGroupBuilder)
        {
            routeGroupBuilder.MapDelete$DomainName$();
        }
    }
}
