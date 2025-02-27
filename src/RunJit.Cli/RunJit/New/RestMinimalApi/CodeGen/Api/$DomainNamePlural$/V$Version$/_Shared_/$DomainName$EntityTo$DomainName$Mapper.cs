using System.Collections.Immutable;
using $ProjectName$.Database.$DomainNamePlural$;
using $ProjectName$.Mapping;

namespace $ProjectName$.Api.$DomainNamePlural$.V1._Shared_
{
    public class $DomainName$EntityTo$DomainName$Mapper : IMapper<$DomainName$Entity, $DomainName$>
    {
        public $DomainName$ MapTo($DomainName$Entity source)
        {
            var $DomainNameLower$ = new $DomainName$()
                          {
                              $PropertyMappings$
                          };

            return $DomainNameLower$;
        }

        public IImmutableList<$DomainName$> MapTo(IEnumerable<$DomainName$Entity> sources)
        {
            return sources.Select(MapTo).ToImmutableList();
        }
    }
}
