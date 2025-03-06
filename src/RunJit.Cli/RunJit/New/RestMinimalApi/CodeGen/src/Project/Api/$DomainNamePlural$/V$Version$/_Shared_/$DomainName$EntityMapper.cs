using System.Collections.Immutable;
using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;

namespace $ProjectName$.Api.$DomainNamePlural$.V1
{
    internal static class Add$DomainName$EntityMapperExtension
    {
        internal static void Add$DomainName$EntityMapper(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<$DomainName$EntityMapper>();
        }
    }

    internal sealed class $DomainName$EntityMapper
    {
        public $DomainName$ MapFrom($DomainName$Entity source)
        {
            var $DomainNameLower$ = new $DomainName$()
                          {
                              $PropertyMappings$
                          };

            return $DomainNameLower$;
        }

        public IImmutableList<$DomainName$> MapFrom(IEnumerable<$DomainName$Entity> sources)
        {
            return sources.Select(MapFrom).ToImmutableList();
        }
    }
}
