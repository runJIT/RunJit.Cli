using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime.CredentialManagement;
using Extensions.Pack;

namespace $ProjectName$.Aws.DynamoDb
{
    internal static class AddAmazonDynamoDbClientFactoryExtension
    {
        internal static void AddAmazonDynamoDbClientFactory(this IServiceCollection services, IConfiguration configuration)
        {
            if (services.IsAlreadyRegistered<IAmazonDynamoDbClientFactory>())
            {
                return;
            }

            AmazonDynamoDBClient dynamoDb;
            var serviceUrl = configuration.GetSection("AWS:DynamoDbServiceUrl").Get<string>();
            if (serviceUrl is null)
            {
                dynamoDb = new AmazonDynamoDBClient();
            }
            else
            {
                var chain = new CredentialProfileStoreChain();
                var profile = configuration.GetSection("AWS:Profile").Get<string>();
                var region = configuration.GetSection("AWS:Region").Get<string>();
                chain.TryGetAWSCredentials(profile, out var credentials);

                var dynamoDbConfig = new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(region), ServiceURL = serviceUrl };
                dynamoDb = new AmazonDynamoDBClient(credentials, dynamoDbConfig);
            }
            services.AddSingletonIfNotExists(dynamoDb);
            services.AddSingletonIfNotExists<IAmazonDynamoDbClientFactory, AmazonDynamoDbClientFactory>();
        }
    }

    public interface IAmazonDynamoDbClientFactory
    {
        DynamoDBContext Create();
    }

    internal sealed class AmazonDynamoDbClientFactory(AmazonDynamoDBClient amazonDynamoDbClient)
        : IAmazonDynamoDbClientFactory
    {
        public DynamoDBContext Create()
        {
            // be careful!!! Never set ownClient to true. This disposes the AmazonDbClient that leads to massive errors. Also do not use the RegionEndpoint before 
            return new DynamoDBContext(amazonDynamoDbClient, new DynamoDBContextConfig
            {
                DisableFetchingTableMetadata = true
            });
        }
    }
}
