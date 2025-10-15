namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class PatchStartup
    {
        internal static void AddPatch(this IServiceCollection services, 
                                      IConfiguration configuration)
        {
            services.AddPatch$DomainName$Command(configuration);
        }
    }
}
