using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;
using $ProjectName$.Mapping;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Create
{
    internal static class AddCreate$DomainName$RequestMapperExtension
    {
        internal static void AddCreate$DomainName$RequestMapper(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Create$DomainName$RequestMapper>();
        }
    }

    internal $DomainName$Entity MapFrom(Create$DomainName$Request source)
    {
        return MapFrom(source, Guid.NewGuid());
    }

    internal sealed class Create$DomainName$RequestMapper
    {
        public $DomainName$Entity MapTo(Create$DomainName$Request source)
        {
            var $DomainNameLower$ = new $DomainName$Entity
                          {
                              $PropertyMappings$
                          };

            return project;
        }
    }
}
