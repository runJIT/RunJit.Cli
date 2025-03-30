using Extensions.Pack;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class AddPatch$DomainName$RequestValidatorExtension
    {
        internal static void AddPatch$DomainName$RequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Patch$DomainName$RequestValidator>();
        }
    }

    internal sealed class Patch$DomainName$RequestValidator(IJsonDiffer jsonDiffer, 
                                                            IJsonSerializer jsonSerializer) : PatchRequestValidator<Patch$DomainName$Request>(jsonDiffer, jsonSerializer)
    {
        protected override IEnumerable<(string PropertyName, ValidationErrorDetails ErrorDetails)> GetValidationErrors(Patch$DomainName$Request request)
        {
            $PatchRequestValidations$
        }
    }
}
