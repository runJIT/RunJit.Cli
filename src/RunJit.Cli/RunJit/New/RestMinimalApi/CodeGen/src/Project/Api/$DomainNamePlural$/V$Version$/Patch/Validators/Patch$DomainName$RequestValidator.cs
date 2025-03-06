using Extensions.Pack;
using $ProjectName$.JsonSerializing;
using $ProjectName$.Validations;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Patch
{
    internal static class AddPatch$DomainName$RequestValidatorExtension
    {
        internal static void AddPatch$DomainName$RequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Patch$DomainName$RequestValidator>();
        }
    }

    internal sealed class Patch$DomainName$RequestValidator(IJsonDiffer jsonDiffer) : PatchRequestValidator<Patch$DomainName$Request>(jsonDiffer)
    {
        protected override IEnumerable<(string PropertyName, string Error)> GetValidationErrors(Patch$DomainName$Request request)
        {
            // Sample: Remove the yield break and replace it with your validation logic
            // if (request.Name.IsNotNullOrWhiteSpace())
            // {
            //     yield return (nameof(request.Name), "Name must not be null, empty or whitespace");
            // }
            //    
            // if (request.Name.Length > 18)
            // {
            //     yield return (nameof(request.Name), "Name must not be longer than 18 characters");
            // }
            yield break;
        }
    }
}
