using System.Collections.Immutable;
using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class Add$DomainName$EntityToPatch$DomainName$RequestMapperExtension
    {
        internal static void Add$DomainName$EntityToPatch$DomainName$RequestMapper(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<$DomainName$EntityToPatch$DomainName$RequestMapper>();
        }
    }

    internal sealed class $DomainName$EntityToPatch$DomainName$RequestMapper
    {
        public Patch$DomainName$Request MapFrom($DomainName$Entity source)
        {
            var $DomainNameLower$ = new Patch$DomainName$Request
            {
                $PropertyMappings$
            };

            return $DomainNameLower$;
        }

        public ImmutableList<Patch$DomainName$Request> MapFrom(IEnumerable<$DomainName$Entity> sources)
        {
            return sources.Select(MapFrom).ToImmutableList();
        }
    }
}
