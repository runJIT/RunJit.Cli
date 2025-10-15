namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class GetByIdStartup
    {
        internal static void AddGetById(this IServiceCollection services, 
                                        IConfiguration configuration)
        {
            services.AddMapGet$DomainName$ByIdEndpoint(configuration);
        }
    }
}
