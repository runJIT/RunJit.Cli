using System.Net;
using Extensions.Pack;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V1
{
    public static class AddDeleteAll$DomainNamePlural$RequestValidatorExtension
    {
        internal static void AddDeleteAll$DomainNamePlural$RequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<DeleteAll$DomainNamePlural$RequestValidator>();
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
    /// Unset query parameters (e.g., omitting <c>$QueryPropertyNameLower$</c>) are treated as not declared and therefore not applied 
    /// to filtering. If a parameter is provided, it must adhere to the expected format (e.g., non-empty, not just whitespace).
    /// </para>
    /// </remarks>
    internal sealed class DeleteAll$DomainNamePlural$RequestValidator(IAttributeValidator attributeValidator) : RequestValidator<DeleteAll$DomainNamePlural$Request>(attributeValidator, HttpStatusCode.BadRequest)
    {
        protected override IEnumerable<PropertyValidationResult> GetValidationErrors(DeleteAll$DomainNamePlural$Request request)
        {
            yield break;
        }
    }
}
