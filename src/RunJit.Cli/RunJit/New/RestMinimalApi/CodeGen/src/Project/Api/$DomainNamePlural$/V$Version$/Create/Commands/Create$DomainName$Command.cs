using Extensions.Pack;
using $ProjectName$.Database.$DomainNamePlural$;
using Siemens.AspNet.ErrorHandling.Contracts;
using Siemens.AspNet.MinimalApi.Sdk.Aws.DynamoDb;

namespace $ProjectName$.Api.$DomainNamePlural$.V1
{
    internal static class AddCreate$DomainName$CommandExtension
    {
        internal static void AddCreate$DomainName$Command(this IServiceCollection services,
                                                                IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddCreate$DomainName$RequestValidator();
            services.AddCreate$DomainName$RequestMapper();
            services.Add$DomainName$EntityMapper();

            services.AddSingletonIfNotExists<Create$DomainName$Command>();
        }
    }

    internal sealed class Create$DomainName$Command(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                                          Create$DomainName$RequestValidator requestValidator,
                                                          Create$DomainName$RequestMapper requestMapper,
                                                          $DomainName$EntityMapper $DomainNameLower$EntityMapper)
    {
        internal async Task<$DomainName$> ExecuteAsync(Create$DomainName$Request create$DomainName$Request,
                                                             CancellationToken cancellationToken)
        {
            // 1. Validate request
            await requestValidator.ValidateAsync(create$DomainName$Request).ConfigureAwait(false);

            // 2. Map the request data to the internal db data (AntiCorruptionLayer ACL)
            var $DomainNameLower$Entity = requestMapper.MapFrom(create$DomainName$Request);

            // 3. Add data into database
            using var dbContext = dynamoDbClientFactory.CreateTenantSpecific();

            // 4. Check if data already exists, to avoid an update with a POST
            var existing$DomainName$ = await dbContext.LoadAsync<$DomainName$Entity>($DomainNameLower$Entity.$IdPropertyName$, cancellationToken).ConfigureAwait(false);
            if (existing$DomainName$.IsNotNull())
            {
                throw new ConflictDetailsException($"{nameof($DomainName$)} already exists.",
                                                   $"The {nameof($DomainName$)} with the {nameof($DomainName$.$IdPropertyName$)} already exists.",
                                                   (nameof($DomainName$.$IdPropertyName$), existing$DomainName$.$IdPropertyName$));
            }

            // 5. Save/Create the data
            await dbContext.SaveAsync($DomainNameLower$Entity, cancellationToken).ConfigureAwait(false);

            // 6. Return created $DomainNameLower$
            var $DomainNameLower$ = $DomainNameLower$EntityMapper.MapFrom($DomainNameLower$Entity);

            return $DomainNameLower$;
        }
    }
}
