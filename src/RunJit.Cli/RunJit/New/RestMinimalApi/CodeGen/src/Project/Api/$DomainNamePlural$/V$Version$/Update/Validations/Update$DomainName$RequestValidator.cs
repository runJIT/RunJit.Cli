using Extensions.Pack;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class AddUpdate$DomainName$RequestValidatorExtension
    {
        internal static void AddUpdate$DomainName$RequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Update$DomainName$RequestValidator>();
        }
    }

    internal sealed class Update$DomainName$RequestValidator(IAttributeValidator attributeValidator) : RequestValidator<Update$DomainName$Request>(attributeValidator)
    {
        protected override IEnumerable<PropertyValidationResult> GetValidationErrors(Update$DomainName$Request request)
        {
            yield break;
        }
    }
}
