using Extensions.Pack;
using $ProjectName$.Database.Projects;

namespace $ProjectName$.Api.Projects.V1.Patch
{
    internal static class AddPatch$DomainName$RequestMapperExtension
    {
        internal static void AddPatch$DomainName$RequestMapper(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Patch$DomainName$RequestMapper>();
        }
    }

    internal sealed class Patch$DomainName$RequestMapper
    {
        public $DomainName$Entity MapFrom(Patch$DomainName$Request source,
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
