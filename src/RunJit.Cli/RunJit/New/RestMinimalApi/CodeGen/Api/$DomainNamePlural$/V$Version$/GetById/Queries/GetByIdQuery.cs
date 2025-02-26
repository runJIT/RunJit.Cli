using System.Collections.Immutable;
using System.Text.Json;
using Extensions.Pack;
using Siemens.AspNet.ErrorHandling.Contracts;
using $ProjectName$.Api.$DomainNamePlural$.V$Version$._Shared_;
using $ProjectName$.Database.$DomainNamePlural$;
using $ProjectName$.Shared.AmazonFactories.DynamoDb;
using $ProjectName$.Shared.OpenTelemetry;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.GetById
{
    internal static class AddGetByIdQueryExtension
    {
        internal static void AddGetByIdQuery(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddTelemetryClientAdapter();
            services.AddSingletonIfNotExists<GetByIdQuery>();
        }
    }

    internal sealed class GetByIdQuery(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                       IMapper<$DomainName$Entity, $DomainName$> mapper)
    {
        internal async Task<$DomainName$> ExecuteAsync(Guid $DomainNameLower$Id, CancellationToken cancellationToken)
        {
            using var dbContext = dynamoDbClientFactory.Create();

            var $DomainNameLower$Entity = await dbContext.LoadAsync<$DomainName$Entity>(projectId, cancellationToken).ConfigureAwait(false) ?? throw new NotFoundDetailsException("Project not found", "The requested Project with the id: {projectId} was not found.", ("projectId", projectId));

            var $DomainNameLower$ = mapper.MapTo($DomainNameLower$Entity);

            return $DomainNameLower$;
        }
    }
}
