using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;
using $ProjectName$.Mapping;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Create
{
    internal static class AddPatch$DomainName$RequestTo$DomainName$EntityMapperExtension
    {
        internal static void AddPatch$DomainName$RequestTo$DomainName$EntityMapper(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRequestMapper<Patch$DomainName$Request, $DomainName$Entity>, Patch$DomainName$RequestTo$DomainName$EntityMapper>();
        }
    }

    internal class Patch$DomainName$RequestTo$DomainName$EntityMapper : IRequestMapper<Patch$DomainName$Request, $DomainName$Entity>
    {
        public $DomainName$Entity MapTo(Patch$DomainName$Request source,
                                   HttpContext httpContext)
        {
            // var stopWatchTotal = Stopwatch.StartNew();
            var $DomainNameLower$ = new $DomainName$Entity
                          {
                              $PropertyMapping$
                          };

            return $DomainNameLower$;
        }
    }
}
