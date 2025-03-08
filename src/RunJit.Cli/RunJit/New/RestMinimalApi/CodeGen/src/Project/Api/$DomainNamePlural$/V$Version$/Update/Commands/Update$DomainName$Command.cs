using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
{
    internal static class AddUpdate$DomainName$CommandExtension
    {
        internal static void AddUpdate$DomainName$Command(this IServiceCollection services,
                                                     IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddUpdate$DomainName$RequestValidator();
            services.AddUpdate$DomainName$RequestMapper();
            services.Add$DomainName$EntityMapper();
            
            services.AddSingletonIfNotExists<Update$DomainName$Command>();
        }
    }

    internal sealed class Update$DomainName$Command(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                               Update$DomainName$RequestValidator requestValidator,
                                               Update$DomainName$RequestMapper requestMapper,
                                               $DomainName$EntityMapper domainMapper)
    {
        internal async Task<$DomainName$> ExecuteAsync(Guid id,
                                                  Update$DomainName$Request update$DomainName$Request,
                                                  CancellationToken cancellationToken)
        {
            // 1. Validate request
            await requestValidator.ValidateAsync(update$DomainName$Request).ConfigureAwait(false);

            // 2. Map the request data to the internal db data (AntiCorruptionLayer ACL)
            var $DomainNameLower$Entity = requestMapper.MapFrom(update$DomainName$Request, id);

            // 3. Add data into database
            using var dbContext = dynamoDbClientFactory.Create();
            await dbContext.SaveAsync($DomainNameLower$Entity, cancellationToken).ConfigureAwait(false);

            // 4. Return created $DomainNameLower$
            var $DomainNameLower$ = domainMapper.MapFrom($DomainNameLower$Entity);

            return $DomainNameLower$;
        }
    }
}
