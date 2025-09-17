namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class CreateStartup
    {
        internal static void AddCreate(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCreate$DomainName$Command(configuration);
            services.AddCreate$DomainName$Endpoint();
        }
    }
}
