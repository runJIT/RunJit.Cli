using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Delete
{
    internal static class AddDelete$DomainName$CommandExtension
    {
        internal static void AddDelete$DomainName$Command(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddSingletonIfNotExists<Delete$DomainName$Command>();
        }
    }

    internal sealed class Delete$DomainName$Command(IAmazonDynamoDbClientFactory dynamoDbClientFactory)
    {
        internal async Task ExecuteAsync(Guid $IdUrlName$, CancellationToken cancellationToken)
        {
            using var dbContext = dynamoDbClientFactory.Create();

            await dbContext.DeleteAsync<$DomainName$>($IdUrlName$, cancellationToken).ConfigureAwait(false);
        }
    }
}
