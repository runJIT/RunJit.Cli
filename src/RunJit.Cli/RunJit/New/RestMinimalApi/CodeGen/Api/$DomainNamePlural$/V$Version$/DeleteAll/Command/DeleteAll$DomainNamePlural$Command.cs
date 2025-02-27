using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;

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
        internal async Task ExecuteAsync(Guid $DomainNameLower$Id, CancellationToken cancellationToken)
        {
            using var dbContext = dynamoDbClientFactory.Create();

            await dbContext.DeleteAsync<$DomainName$>($DomainNameLower$Id, cancellationToken).ConfigureAwait(false);
        }
    }
}
