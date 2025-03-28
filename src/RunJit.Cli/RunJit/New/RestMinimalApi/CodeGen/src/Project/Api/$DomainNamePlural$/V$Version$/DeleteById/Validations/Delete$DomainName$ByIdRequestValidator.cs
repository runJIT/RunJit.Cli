using System.Net;
using Extensions.Pack;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V1
{
    public static class AddDelete$DomainName$ByIdRequestValidatorExtension
    {
        internal static void AddDelete$DomainName$ByIdRequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Delete$DomainName$ByIdRequestValidator>();
        }
    }

    /// <summary>
    /// Validates query parameters for the DELETE request to delete the $DomainNamePlural$.
    /// Returns a 400 Bad Request for invalid parameters per 
    /// <see href="https://www.rfc-editor.org/rfc/rfc9110#status.400">RFC 9110</see>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For request bodies that are syntactically correct yet semantically invalid (e.g., in POST, PUT, PATCH), 
    /// a 422 Unprocessable Content status is recommended 
    /// (<see href="https://www.rfc-editor.org/rfc/rfc9110#status.422">RFC 9110</see>).
    /// </para>
    /// <para>
    /// Unset query parameters (e.g., omitting <c>title</c>) are treated as not declared and therefore not applied 
    /// to filtering. If a parameter is provided, it must adhere to the expected format (e.g., non-empty, not just whitespace).
    /// </para>
    /// </remarks>
    internal sealed class Delete$DomainName$ByIdRequestValidator() : RequestValidator<Delete$DomainName$ByIdRequest>(HttpStatusCode.BadRequest)
    {
        protected override IEnumerable<(string PropertyName, ValidationErrorDetails ErrorDetails)> GetValidationErrors(Delete$DomainName$ByIdRequest request)
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
