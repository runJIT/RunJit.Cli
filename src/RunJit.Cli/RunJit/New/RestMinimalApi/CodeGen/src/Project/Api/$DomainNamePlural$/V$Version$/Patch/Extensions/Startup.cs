namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Patch
{
    internal static class Startup
    {
        internal static void AddPatch$DomainName$(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddPatch$DomainName$Command(configuration);
        }

        internal static void MapPatch$DomainName$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPatch$DomainName$();
        }
    }
}
