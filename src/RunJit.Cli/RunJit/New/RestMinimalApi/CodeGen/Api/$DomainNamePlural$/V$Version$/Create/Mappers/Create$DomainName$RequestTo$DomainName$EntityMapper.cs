using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;
using $ProjectName$.Mapping;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Create
{
    internal static class AddCreate$DomainName$RequestTo$DomainName$EntityMapperExtension
    {
        internal static void AddCreate$DomainName$RequestTo$DomainName$EntityMapper(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRequestMapper<Create$DomainName$Request, $DomainName$Entity>, Create$DomainName$RequestTo$DomainName$EntityMapper>();
        }
    }

    internal class Create$DomainName$RequestTo$DomainName$EntityMapper : IRequestMapper<Create$DomainName$Request, $DomainName$Entity>
    {
        public $DomainName$Entity MapTo(Create$DomainName$Request source,
                                   HttpContext httpContext)
        {
            // var stopWatchTotal = Stopwatch.StartNew();
            var $DomainNameLower$ = new $DomainName$Entity
                          {
                              $PropertyMappings$
                          };

            return project;
        }
    }
}
