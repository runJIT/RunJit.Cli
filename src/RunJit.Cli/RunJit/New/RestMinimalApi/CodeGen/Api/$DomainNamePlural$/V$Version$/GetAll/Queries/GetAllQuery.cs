using System.Collections.Immutable;
using Amazon.DynamoDBv2.DocumentModel;
using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;
using $ProjectName$.Database.Projects;
using $ProjectName$.Mapping;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetAll
{
    internal static class AddGetAllQueryExtension
    {
        internal static void AddGetAllQuery(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddSingletonIfNotExists<GetAllQuery>();
        }
    }

    internal sealed class GetAllQuery(IAmazonDynamoDbClientFactory amazonDynamoDbClientFactory,
                                      IMapper<$DomainName$Entity, $DomainName$> mapper)
    {
        internal async Task<IImmutableList<$DomainName$>> ExecuteAsync()
        {
            
            using var dbContext = amazonDynamoDbClientFactory.Create();

            var queryConfig = new ScanOperationConfig();
            
            var queryResults = await dbContext.FromScanAsync<$DomainName$Entity>(queryConfig).GetRemainingAsync().ConfigureAwait(false);

            var $DomainNamePluralLower$ = mapper.MapTo(queryResults);

            return $DomainNamePluralLower$;
        }
    }
}
