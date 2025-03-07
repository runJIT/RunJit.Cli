using $ProjectName$.Api.$DomainNamePlural$.V$Version$.Create;
using $ProjectName$.Api.$DomainNamePlural$.V$Version$.Delete;
using $ProjectName$.Api.$DomainNamePlural$.V$Version$.DeleteAll;
using $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetAll;
using $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetById;
using $ProjectName$.Api.$DomainNamePlural$.V$Version$.Patch;
using $ProjectName$.Api.$DomainNamePlural$.V$Version$.Update;

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

        internal static void Map$DomainNamePlural$V$Version$(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapCreate();
            endpointRouteBuilder.MapDeleteAll();
            endpointRouteBuilder.MapDeleteById();
            endpointRouteBuilder.MapGetAll();
            endpointRouteBuilder.MapGetById();
            endpointRouteBuilder.MapPatch();
            endpointRouteBuilder.MapUpdate();
        }
    }
}
