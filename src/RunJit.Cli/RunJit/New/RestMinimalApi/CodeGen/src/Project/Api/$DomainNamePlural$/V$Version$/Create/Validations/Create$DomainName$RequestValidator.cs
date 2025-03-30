using Extensions.Pack;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    public static class AddCreate$DomainName$RequestValidatorExtension
    {
        internal static void AddCreate$DomainName$RequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Create$DomainName$RequestValidator>();
        }
    }

    internal sealed class Create$DomainName$RequestValidator : RequestValidator<Create$DomainName$Request>
    {
        protected override IEnumerable<(string PropertyName, ValidationErrorDetails ErrorDetails)> GetValidationErrors(Create$DomainName$Request request)
        {
            $CreateRequestValidations$
        }
    }
}
