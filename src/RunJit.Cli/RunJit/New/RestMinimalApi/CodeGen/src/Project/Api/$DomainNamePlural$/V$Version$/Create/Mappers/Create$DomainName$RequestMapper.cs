using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Create
{
    internal static class AddCreate$DomainName$RequestMapperExtension
    {
        internal static void AddCreate$DomainName$RequestMapper(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Create$DomainName$RequestMapper>();
        }
    }

    internal sealed class Create$DomainName$RequestMapper
    {
        public $DomainName$Entity MapFrom(Create$DomainName$Request source)
        {
            return MapFrom(source, Guid.NewGuid());
        }

        public $DomainName$Entity MapFrom(Create$DomainName$Request source,
                                          Guid $IdPropertyNameLower$)
        {
            var $DomainNameLower$ = new $DomainName$Entity
            {
                $IdPropertyName$ = $IdPropertyNameLower$,
                $PropertyMappings$
            };

            return $DomainNameLower$;
        }
    }
}
