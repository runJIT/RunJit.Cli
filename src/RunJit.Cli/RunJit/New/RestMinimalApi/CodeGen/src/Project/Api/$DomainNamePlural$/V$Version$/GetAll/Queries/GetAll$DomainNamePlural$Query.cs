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
        internal async Task<IImmutableList<$DomainName$>> ExecuteAsync(GetAll$DomainNamePlural$Request request,
                                                                             CancellationToken cancellationToken)
        {
            // 1. Validate the request
            await requestValidator.ValidateAsync(request).ConfigureAwait(false);

            // 2. Create dynamo db context
            using var dbContext = dynamoDbClientFactory.CreateTenantSpecific();

            // 3. Get all $DomainNameLower$ entities by filter criteria or all
            var $DomainNameLower$Entities = await dbContext.GetAllAsync<$DomainName$Entity>([(nameof($DomainName$Entity.$QueryPropertyName$), request.$QueryPropertyName$)], cancellationToken).ConfigureAwait(false);

            // 4. Map to api models (AntiCorruptionLayer - ACL)
            var $DomainNamePluralLower$ = mapper.MapFrom($DomainNameLower$Entities);

            // 5. Return the mapped objects
            return $DomainNamePluralLower$;
        }
    }
}
