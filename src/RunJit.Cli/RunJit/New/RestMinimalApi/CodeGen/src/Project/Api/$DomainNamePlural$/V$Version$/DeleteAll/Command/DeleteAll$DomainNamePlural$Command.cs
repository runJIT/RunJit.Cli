using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;
using $ProjectName$.Database.$DomainNamePlural$;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.DeleteAll
{
    internal static class AddDeleteAll$DomainNamePlural$CommandExtension
    {
        internal static void AddDeleteAll$DomainNamePlural$Command(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddSingletonIfNotExists<DeleteAll$DomainNamePlural$Command>();
        }
    }

    internal sealed class DeleteAll$DomainNamePlural$Command(IAmazonDynamoDbClientFactory dynamoDbClientFactory)
    {
        internal async Task ExecuteAsync(string $QueryPropertyNameLower$,
                                         CancellationToken cancellationToken)
        {
            // 1. Create dynamo db context
            using var dbContext = dynamoDbClientFactory.Create();

            // 2. Delete all projects or those which are matching the filter criteria
            await dbContext.DeleteAllAsync<$DomainName$Entity>($QueryPropertyNameLower$, cancellationToken).ConfigureAwait(false);
        }
    }
}
