using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk.Aws.DynamoDb;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class AddGet$DomainName$ByIdQueryExtension
    {
        internal static void AddGet$DomainName$ByIdQuery(this IServiceCollection services,
                                                               IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.Add$DomainName$EntityMapper();
            services.AddGet$DomainName$ByIdRequestValidator();

            services.AddSingletonIfNotExists<Get$DomainName$ByIdQuery>();
        }
    }

    internal sealed class Get$DomainName$ByIdQuery(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                                         $DomainName$EntityMapper mapper,
                                                         Get$DomainName$ByIdRequestValidator requestValidator)
    {
        internal async Task<$DomainName$> ExecuteAsync(Get$DomainName$ByIdRequest request,
                                                             CancellationToken cancellationToken)
        {
            await requestValidator.ValidateAsync(request).ConfigureAwait(false);
                
            using var dbContext = dynamoDbClientFactory.CreateTenantSpecific();

            var $DomainNameLower$Entity = await dbContext.GetByIdAsync<$DomainName$Entity>(request.$IdPropertyName$, cancellationToken).ConfigureAwait(false);

            if ($DomainNameLower$Entity.IsNull())
            {
                throw new NotFoundDetailsException("$DomainName$ not found",
                                                   $"The requested {nameof($DomainName$)} with the {nameof($DomainName$.$IdPropertyName$)}: {request.$IdPropertyName$} was not found or does not exist any more.",
                                                   ("$IdPropertyName$", request.$IdPropertyName$));
            }

            var $DomainNameLower$ = mapper.MapFrom($DomainNameLower$Entity);

            return $DomainNameLower$;
        }
    }
}
