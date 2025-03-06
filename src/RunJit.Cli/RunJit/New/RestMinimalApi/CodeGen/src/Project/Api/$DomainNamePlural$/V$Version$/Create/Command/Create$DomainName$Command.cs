using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;
using $ProjectName$.Validations;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Create
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
            using var dbContext = dynamoDbClientFactory.Create();
            await dbContext.SaveAsync($DomainNameLower$Entity, cancellationToken).ConfigureAwait(false);

            // 4. Return created $DomainNameLower$
            var $DomainNameLower$ = $DomainNameLower$EntityMapper.MapFrom($DomainNameLower$Entity);

            return $DomainNameLower$;
        }
    }
}
