using System.Net;
using Extensions.Pack;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;
using Siemens.AspNet.MinimalApi.Sdk.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class AddPatch$DomainName$RequestValidatorExtension
    {
        internal static void AddPatch$DomainName$RequestValidator(this IServiceCollection services)
        {
            services.AddJsonDiffer();
            services.AddJsonSerializer();
            
            services.AddSingletonIfNotExists<Patch$DomainName$RequestValidator>();
        }
    }

    internal sealed class Patch$DomainName$RequestValidator(IJsonDiffer jsonDiffer, 
                                                            IJsonSerializer jsonSerializer,
                                                            IAttributeValidator attributeValidator) : PatchRequestValidator<Patch$DomainName$Request>(jsonDiffer, jsonSerializer, attributeValidator)
    {
        protected override IEnumerable<PropertyValidationResult> GetValidationErrors(Patch$DomainName$Request request)
        {
            yield break;
        }
    }
}
