using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Update
{
    internal static class AddUpdate$DomainName$RequestMapperExtension
    {
        internal static void AddUpdate$DomainName$RequestMapper(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Update$DomainName$RequestMapper>();
        }
    }

    internal sealed class Update$DomainName$RequestMapper
    {
        public $DomainName$Entity MapFrom(Update$DomainName$Request source)
        {
            return MapFrom(source, Guid.NewGuid());
        }

        public $DomainName$Entity MapFrom(Update$DomainName$Request source,
                                     Guid id)
        {
            var $DomainNameLower$ = new $DomainName$Entity
                          { 
                              $IdPropertyName$ = id,
                              $PropertyMappings$
                          };

            return $DomainNameLower$;
        }
    }
}
