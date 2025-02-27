using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;
using $ProjectName$.Database.Projects;
using $ProjectName$.Mapping;
using $ProjectName$.Validations;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Update
{
    internal static class AddUpdate$DomainName$CommandExtension
    {
        internal static void AddUpdate$DomainName$Command(this IServiceCollection services,
                                                     IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddSingletonIfNotExists<Update$DomainName$Command>();
        }
    }

    internal sealed class Update$DomainName$Command(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                                    IRequestValidator<Update$DomainName$Request> requestValidator,
                                                    IRequestMapper<Update$DomainName$Request, $DomainName$Entity> requestMapper,
                                                    IMapper<$DomainName$Entity, $DomainName$> domainMapper)
    {
        internal async Task<$DomainName$> ExecuteAsync(HttpContext httpContext,
                                                  Guid $DomainNameLower$Id,
                                                  Update$DomainName$Request update$DomainName$Request,
                                                  CancellationToken cancellationToken)
        {
            // 1. Validate request
            await requestValidator.ValidateAsync(update$DomainName$Request).ConfigureAwait(false);

            // 2. Map the request data to the internal db data (ACL)
            var $DomainNameLower$Entity = requestMapper.MapTo(update$DomainName$Request, httpContext);

            // 3. Add data into database
            using var dbContext = dynamoDbClientFactory.Create();
            await dbContext.SaveAsync($DomainNameLower$Entity, cancellationToken).ConfigureAwait(false);

            // 4. Return created $DomainNameLower$
            var $DomainNameLower$ = domainMapper.MapTo($DomainNameLower$Entity);

            return $DomainNameLower$;
        }
    }
}
