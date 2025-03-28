using System.Net;
using Extensions.Pack;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V1
{
    public static class AddGet$DomainName$ByIdRequestValidatorExtension
    {
        internal static void AddGet$DomainName$ByIdRequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Get$DomainName$ByIdRequestValidator>();
        }
    }


    /// <summary>
    /// Validates url parameters for the GET by id request to fetch the forms configuration by its id
    /// Returns a 400 Bad Request for invalid parameters per 
    /// <see href="https://www.rfc-editor.org/rfc/rfc9110#status.400">RFC 9110</see>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For request bodies that are syntactically correct yet semantically invalid (e.g., in POST, PUT, PATCH), 
    /// a 422 Unprocessable Content status is recommended 
    /// (<see href="https://www.rfc-editor.org/rfc/rfc9110#status.422">RFC 9110</see>).
    /// </para>
    /// </remarks>
    internal sealed class Get$DomainName$ByIdRequestValidator() : RequestValidator<Get$DomainName$ByIdRequest>(HttpStatusCode.BadRequest)
    {
        protected override IEnumerable<(string PropertyName, ValidationErrorDetails ErrorDetails)> GetValidationErrors(Get$DomainName$ByIdRequest request)
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
