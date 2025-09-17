namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class Startup
    {
        internal static void Add$DomainNamePlural$V$Version$(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddCreate(configuration);
            serviceCollection.AddDeleteAll(configuration);
            serviceCollection.AddDeleteById(configuration);
            serviceCollection.AddGetAll(configuration);
            serviceCollection.AddGetById(configuration);
            serviceCollection.AddPatch(configuration);
            serviceCollection.AddUpdate(configuration);
        }
    }
}
