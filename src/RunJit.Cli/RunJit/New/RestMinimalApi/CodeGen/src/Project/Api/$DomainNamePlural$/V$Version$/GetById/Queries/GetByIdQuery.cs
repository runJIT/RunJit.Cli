using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;
using $ProjectName$.Database.Projects;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetById
{
    internal static class AddGetByIdQueryExtension
    {
        internal static void AddGetByIdQuery(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.Add$DomainName$EntityMapper();
            
            services.AddSingletonIfNotExists<GetByIdQuery>();
        }
    }

    internal sealed class GetByIdQuery(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                       $DomainName$EntityMapper mapper)
    {
        internal async Task<$DomainName$> ExecuteAsync(Guid $IdUrlName$, CancellationToken cancellationToken)
        {
            using var dbContext = dynamoDbClientFactory.Create();

            var $DomainNameLower$Entity = await dbContext.LoadAsync<$DomainName$Entity>($IdUrlName$, cancellationToken).ConfigureAwait(false) ?? throw new NotFoundDetailsException("Project not found", "The requested Project with the id: {projectId} was not found.", ("projectId", projectId));

            var $DomainNameLower$ = mapper.MapTo($DomainNameLower$Entity);

            return $DomainNameLower$;
        }
    }
}
