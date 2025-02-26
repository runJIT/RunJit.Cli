namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Update
{
    internal static class Startup
    {
        internal static void AddUpdate$DomainName$(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddUpdate$DomainName$Command(configuration);
        }

        internal static void UseUpdate$DomainName$(this IEndpointRouteBuilder routeGroupBuilder)
        {
            routeGroupBuilder.MapUpdate$DomainName$();
        }
    }
}
