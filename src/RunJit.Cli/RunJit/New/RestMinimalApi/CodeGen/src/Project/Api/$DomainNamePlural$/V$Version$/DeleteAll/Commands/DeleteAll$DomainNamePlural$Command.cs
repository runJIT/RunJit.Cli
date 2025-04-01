using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;
using Siemens.AspNet.MinimalApi.Sdk.Aws.DynamoDb;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class AddDeleteAll$DomainNamePlural$CommandExtension
    {
        internal static void AddDeleteAll$DomainNamePlural$Command(this IServiceCollection services,
                                                                    IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddDeleteAll$DomainNamePlural$RequestValidator();
                
            services.AddSingletonIfNotExists<DeleteAll$DomainNamePlural$Command>();
        }
    }

    internal sealed class DeleteAll$DomainNamePlural$Command(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                                              DeleteAll$DomainNamePlural$RequestValidator validator)
    {
        internal async Task ExecuteAsync(DeleteAll$DomainNamePlural$Request request,
                                         CancellationToken cancellationToken)
        {
            // 1. Validate the delete request
            await validator.ValidateAsync(request).ConfigureAwait(false);
                
            // 2. Create dynamo db context
            using var dbContext = dynamoDbClientFactory.CreateTenantSpecific();

            // 3. Delete all projects or those which are matching the filter criteria
            await dbContext.DeleteAllAsync<$DomainName$Entity>([(nameof($DomainName$Entity.$QueryPropertyName$), request.$QueryPropertyName$)], cancellationToken).ConfigureAwait(false);
        }
    }
}
