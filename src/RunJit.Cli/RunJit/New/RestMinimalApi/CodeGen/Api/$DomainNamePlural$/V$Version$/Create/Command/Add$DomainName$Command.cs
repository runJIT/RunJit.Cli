using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;
using $ProjectName$.Database.$DomainNamePlural$;
using $ProjectName$.Mapping;
using $ProjectName$.Validations;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Create
{
    internal static class AddCreate$DomainName$CommandExtension
    {
        internal static void AddCreate$DomainName$Command(this IServiceCollection services,
                                                     IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddSingletonIfNotExists<Create$DomainName$Command>();
        }
    }

    // Domain command Create project
    internal sealed class Create$DomainName$Command(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                               IRequestValidator<Create$DomainName$Request> requestValidator,
                                               IRequestMapper<Create$DomainName$Request, $DomainName$Entity> requestMapper,
                                               IMapper<$DomainName$Entity, $DomainName$> domainMapper)
    {
        internal async Task<$DomainName$> ExecuteAsync(HttpContext httpContext,
                                                       Create$DomainName$Request create$DomainName$Request,
                                                       CancellationToken cancellationToken)
        {
            // 1. Validate request
            await requestValidator.ValidateAsync(create$DomainName$Request).ConfigureAwait(false);

            // 2. Map the request data to the internal db data (ACL)
            var $DomainNameLower$Entity = requestMapper.MapTo(create$DomainName$Request, httpContext);

            // 3. Add data into database
            using var dbContext = dynamoDbClientFactory.Create();
            await dbContext.SaveAsync($DomainNameLower$Entity, cancellationToken).ConfigureAwait(false);

            // 4. Return created project
            var $DomainNameLower$ = domainMapper.MapTo($DomainNameLower$Entity);

            return $DomainNameLower$;
        }
    }
}
