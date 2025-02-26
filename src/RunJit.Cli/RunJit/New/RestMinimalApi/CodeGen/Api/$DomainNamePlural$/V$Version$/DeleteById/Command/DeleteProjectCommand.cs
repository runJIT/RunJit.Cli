using Extensions.Pack;
using $ProjectName$.Api.Projects.V$Version$._Shared_;
using $ProjectName$.Shared.AmazonFactories.DynamoDb;
using $ProjectName$.Shared.OpenTelemetry;

namespace $ProjectName$.Api.Projects.V$Version$.Delete
{
    internal static class AddDeleteProjectCommandExtension
    {
        internal static void AddDeleteProjectCommand(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddTelemetryClientAdapter();
            services.AddSingletonIfNotExists<DeleteProjectCommand>();
        }
    }

    internal sealed class DeleteProjectCommand(IAmazonDynamoDbClientFactory dynamoDbClientFactory)
    {
        internal async Task ExecuteAsync(Guid projectId, CancellationToken cancellationToken)
        {
            using var dbContext = dynamoDbClientFactory.Create();

            await dbContext.DeleteAsync<Project>(projectId, cancellationToken).ConfigureAwait(false);
        }
    }
}
