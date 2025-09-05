using $ProjectName$.Api.$DomainNamePlural$.V$Version$;

namespace $ProjectName$.Api.$DomainNamePlural$
{
    internal static class Startup
    {
        internal static void Add$DomainNamePlural$(this IServiceCollection services, IConfiguration configuration)
        {
            services.Add$DomainNamePlural$V$Version$(configuration);
        }
    }
}
