using Extensions.Pack;
using $ProjectName$.Api.$DomainNamePlural$.V1.Create;
using $ProjectName$.Database.$DomainNamePlural$;
using $ProjectName$.Mapping;

namespace $ProjectName$.Api.$DomainNamePlural$.V1.Update
{
    internal static class AddUpdate$DomainName$RequestTo$DomainName$EntityMapperExtension
    {
        internal static void AddUpdate$DomainName$RequestTo$DomainName$EntityMapper(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRequestMapper<Create$DomainName$Request, $DomainName$Entity>, Update$DomainName$RequestTo$DomainName$EntityMapper>();
        }
    }

    internal sealed class Update$DomainName$RequestTo$DomainName$EntityMapper : IRequestMapper<Create$DomainName$Request, $DomainName$Entity>
    {
        public $DomainName$Entity MapTo(Create$DomainName$Request source,
                                   HttpContext httpContext)
        {
            var $DomainNameLower$ = new $DomainName$Entity
            {
                $PropertyMappings$
            };

            return project;
        }
    }
}
