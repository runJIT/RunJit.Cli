using System.Net;
using Extensions.Pack;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;
using Siemens.AspNet.MinimalApi.Sdk.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    public static class AddCreate$DomainName$RequestValidatorExtension
    {
        internal static void AddCreate$DomainName$RequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Create$DomainName$RequestValidator>();
        }
    }

    internal sealed class Create$DomainName$RequestValidator(IAttributeValidator attributeValidator) : RequestValidator<Create$DomainName$Request>(attributeValidator)
    {
        protected override IEnumerable<PropertyValidationResult> GetValidationErrors(Create$DomainName$Request request)
        {
            yield break;
        }
    }
}
