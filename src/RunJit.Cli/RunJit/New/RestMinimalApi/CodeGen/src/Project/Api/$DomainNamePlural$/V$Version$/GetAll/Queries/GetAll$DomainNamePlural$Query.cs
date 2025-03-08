using System.Collections.Immutable;
using Amazon.DynamoDBv2.DocumentModel;
using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;
using $ProjectName$.Database.$DomainNamePlural$;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class AddGetAll$DomainNamePlural$QueryExtension
    {
        internal static void AddGetAll$DomainNamePlural$Query(this IServiceCollection services,
                                            IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.Add$DomainName$EntityMapper();

            services.AddSingletonIfNotExists<GetAll$DomainNamePlural$Query>();
        }
    }

    internal sealed class GetAll$DomainNamePlural$Query(IAmazonDynamoDbClientFactory amazonDynamoDbClientFactory,
                                      $DomainName$EntityMapper mapper)
    {
        internal async Task<IImmutableList<$DomainName$>> ExecuteAsync(string $QueryPropertyNameLower$,
                                                                       CancellationToken cancellationToken)
        {
            // 1. Create dynamo db context
            using var dbContext = amazonDynamoDbClientFactory.Create();

            // 2. Get all $DomainNameLower$ entities by filter criteria or all
            var $DomainNameLower$Entities = await dbContext.GetAllAsync<$DomainName$Entity>([(nameof($DomainName$Entity.$QueryPropertyName$), $QueryPropertyNameLower$)], cancellationToken).ConfigureAwait(false);

            // 3. Map to api models (AntiCorruptionLayer - ACL)
            var $DomainNamePluralLower$ = mapper.MapFrom($DomainNameLower$Entities);

            // 4. Return the mapped objects
            return $DomainNamePluralLower$;
        }
    }
}
