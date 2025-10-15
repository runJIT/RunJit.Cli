namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class GetAllStartup
    {
        internal static void AddGetAll(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMapGetAll$DomainNamePluralLower$Endpoint();
        }
    }
}
