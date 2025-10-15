using System.Net;
using Extensions.Pack;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;
using Siemens.AspNet.MinimalApi.Sdk.Contracts;

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
    internal sealed class Get$DomainName$ByIdRequestValidator(IAttributeValidator attributeValidator) : RequestValidator<Get$DomainName$ByIdRequest>(attributeValidator, HttpStatusCode.BadRequest)
    {
        protected override IEnumerable<PropertyValidationResult> GetValidationErrors(Get$DomainName$ByIdRequest request)
        {
            yield break;
        }
    }
}
