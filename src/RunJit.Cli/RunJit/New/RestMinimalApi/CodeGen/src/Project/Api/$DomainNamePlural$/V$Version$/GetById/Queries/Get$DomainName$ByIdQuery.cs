using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;
using $ProjectName$.Database.$DomainNamePlural$;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetById
{
    internal static class AddGet$DomainName$ByIdQueryExtension
    {
        internal static void AddGet$DomainName$ByIdQuery(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.Add$DomainName$EntityMapper();
            
            services.AddSingletonIfNotExists<Get$DomainName$ByIdQuery>();
        }
    }

    internal sealed class Get$DomainName$ByIdQuery(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                       $DomainName$EntityMapper mapper)
    {
        internal async Task<$DomainName$> ExecuteAsync(Guid $IdUrlName$, CancellationToken cancellationToken)
        {
            using var dbContext = dynamoDbClientFactory.Create();

            var $DomainNameLower$Entity = await dbContext.LoadAsync<$DomainName$Entity>($IdUrlName$, cancellationToken).ConfigureAwait(false) ?? throw new NotFoundDetailsException("Project not found", "The requested Project with the id: {projectId} was not found.", ("projectId", projectId));

            var $DomainNameLower$ = mapper.MapFrom($DomainNameLower$Entity);

            return $DomainNameLower$;
        }
    }
}
