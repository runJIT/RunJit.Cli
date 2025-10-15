namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class DeleteAllStartup
    {
        internal static void AddDeleteAll(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDelete$DomainNamePlural$Endpoint();
        }
    }
}
