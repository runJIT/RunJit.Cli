using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;
using Siemens.AspNet.MinimalApi.Sdk.Aws.DynamoDb;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class AddDelete$DomainName$CommandExtension
    {
        internal static void AddDelete$DomainName$Command(this IServiceCollection services,
                                                                IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddDelete$DomainName$ByIdRequestValidator();
                
            services.AddSingletonIfNotExists<Delete$DomainName$Command>();
        }
    }

    internal sealed class Delete$DomainName$Command(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                                          Delete$DomainName$ByIdRequestValidator requestValidator)
    {
        internal async Task ExecuteAsync(Delete$DomainName$ByIdRequest request,
                                         CancellationToken cancellationToken)
        {
            // 1. Validate the delete request
            requestValidator.Validate(request);
                
            // 2. Create dynamo db context
            using var dbContext = dynamoDbClientFactory.CreateTenantSpecific();
            
            // 3. Delete project by its id
            await dbContext.DeleteByIdAsync<$DomainName$Entity>(request.$IdPropertyName$, cancellationToken).ConfigureAwait(false);
        }
    }
}
