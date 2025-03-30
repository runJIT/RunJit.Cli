using System.Net;
using Extensions.Pack;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk;

namespace $ProjectName$.Api.$DomainNamePlural$.V1
{
    public static class AddGetAll$DomainNamePlural$RequestValidatorExtension
    {
        internal static void AddGetAll$DomainNamePlural$RequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<GetAll$DomainNamePlural$RequestValidator>();
        }
    }

    /// <summary>
    /// Validates query parameters for the GET request retrieving $DomainNamePlural$.
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
    internal sealed class GetAll$DomainNamePlural$RequestValidator() : RequestValidator<GetAll$DomainNamePlural$Request>(HttpStatusCode.BadRequest)
    {
        protected override IEnumerable<(string PropertyName, ValidationErrorDetails ErrorDetails)> GetValidationErrors(GetAll$DomainNamePlural$Request request)
        {
            // Request:
            // 
            // An unset query parameter will be NULL in this case. Which is for us the sign the query parameter
            // was not declared and should not be used !
            // 
            //if (request.$QueryPropertyName$ is null)
            //{
            //    yield return (nameof(request.$QueryPropertyName$), $"{nameof(request.$QueryPropertyName$)} must not be null. To filter results, provide a valid $QueryPropertyNameLower$ (e.g., GET /$DomainNamePluralLower$?$QueryPropertyNameLower$=My$QueryPropertyName$). If no filter is desired, omit the parameter (e.g., GET /$DomainNamePluralLower$).");
            //}

            $GetAllRequestValidations$
        }
    }
}
