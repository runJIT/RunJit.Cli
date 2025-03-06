using Extensions.Pack;
using $ProjectName$.Validations;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Update
{
    internal static class AddUpdate$DomainName$RequestValidatorExtension
    {
        internal static void AddUpdate$DomainName$RequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Update$DomainName$RequestValidator>();
        }
    }

    internal sealed class Update$DomainName$RequestValidator : RequestValidator<Update$DomainName$Request>
    {
        protected override IEnumerable<(string PropertyName, string Error)> GetValidationErrors(Update$DomainName$Request request)
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
