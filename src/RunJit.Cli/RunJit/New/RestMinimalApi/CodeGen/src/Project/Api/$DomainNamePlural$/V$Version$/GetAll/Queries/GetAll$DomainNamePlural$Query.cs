using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Immutable;
using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;
using Siemens.AspNet.MinimalApi.Sdk.Aws.DynamoDb;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{

    internal static class AddGetAll$DomainNamePlural$QueryExtension
    {
        internal static void AddGetAll$DomainNamePlural$Query(this IServiceCollection services,
                                                               IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddGetAll$DomainNamePlural$RequestValidator();
            services.Add$DomainName$EntityMapper();

            services.AddSingletonIfNotExists<GetAll$DomainNamePlural$Query>();
        }
    }

    internal sealed class GetAll$DomainNamePlural$Query(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                                         GetAll$DomainNamePlural$RequestValidator requestValidator,
                                                         $DomainName$EntityMapper mapper)
    {
        internal async Task<ImmutableList<$DomainName$>> ExecuteAsync(GetAll$DomainNamePlural$Request request,
                                                                             CancellationToken cancellationToken)
        {
            // 1. Validate the request
            requestValidator.Validate(request);

            // 2. Setup scan configuration to delete all matching items
            var filters = new List<ScanCondition>();

            if (request.$QueryPropertyName$.IsNotNullOrWhiteSpace())
            {
                filters.Add(new ScanCondition(nameof(request.$QueryPropertyName$), ScanOperator.Equal, request.$QueryPropertyName$));
            }
            
            // 3. Create dynamo db context
            using var dbContext = dynamoDbClientFactory.CreateTenantSpecific();

            // 4. Get all $DomainNameLower$ entities by filter criteria or all
            var $DomainNameLower$Entities = await dbContext.ScanAsync<$DomainName$Entity>(filters.ToImmutableList(), cancellationToken).ConfigureAwait(false);

            // 5. Map to api models (AntiCorruptionLayer - ACL)
            var $DomainNamePluralLower$ = mapper.MapFrom($DomainNameLower$Entities);

            // 6. Return the mapped objects
            return $DomainNamePluralLower$;
        }
    }
}
