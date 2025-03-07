using $ProjectName$.Api.$DomainNamePlural$.V$Version$;

namespace $ProjectName$.Api.$DomainNamePlural$
{
    internal static class Startup
    {
        internal static void Add$DomainNamePlural$(this IServiceCollection services, IConfiguration configuration)
        {
            services.Add$DomainNamePlural$V1(configuration);
        }

        internal static void Map$DomainNamePlural$(this IEndpointRouteBuilder endpoints)
        {
            endpoints.Map$DomainNamePlural$V1();
        }
    }
}
