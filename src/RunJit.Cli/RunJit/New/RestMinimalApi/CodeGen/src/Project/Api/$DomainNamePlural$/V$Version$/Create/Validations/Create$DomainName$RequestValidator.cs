using Extensions.Pack;
using $ProjectName$.Validations;

namespace $ProjectName$.Api.Projects.V$Version$.Create
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
        protected override IEnumerable<(string PropertyName, string Error)> GetValidationErrors(Create$DomainName$Request request)
        {
            // Sample: Remove the yield break and replace it with your validation logic
            //
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
