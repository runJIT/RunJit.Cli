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
            // Sample: Remove the yield break and replace it with your validation logic
            //
            // if (request.FormsId.IsNull())
            // {
            //     var errorDetails = new ValidationErrorDetails()
            //                        {
            //                            CurrentValue = request.FormsId,
            //                            Errors = [$"{nameof(request.FormsId)} must not be null"],
            //                            Samples = ["This is a cool project", "Hello World"],
            //                        };
               
            //     yield return (nameof(request.FormsId), errorDetails);
            // }
            // 
            // if (request.FormsId.IsEmpty())
            // {
            //     var errorDetails = new ValidationErrorDetails()
            //                        {
            //                            CurrentValue = request.FormsId,
            //                            Errors = [$"{nameof(request.FormsId)} must not be empty"],
            //                            Samples = ["This is a cool project", "Hello World"],
            //                        };
               
            //     yield return (nameof(request.FormsId), errorDetails);
            // }
            yield break;
        }
    }
}
