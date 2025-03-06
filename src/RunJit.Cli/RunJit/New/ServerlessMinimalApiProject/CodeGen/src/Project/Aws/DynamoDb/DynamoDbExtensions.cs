using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Extensions.Pack;
using $ProjectName$.Database.Projects;

namespace $ProjectName$.Aws.DynamoDb
{
    internal static class DynamoDbExtensions
    {
        internal static async Task DeleteAll<TEntity>(this DynamoDBContext dynamoDbContext,
                                                      string queryProperty = "",
                                                      CancellationToken cancellationToken = default) where TEntity : class
        {
            var scanConfig = new ScanOperationConfig
            {
                Filter = new ScanFilter()
            };

            if (queryProperty.IsNotNullOrWhiteSpace())
            {
                scanConfig.Filter.AddCondition(nameof(ProjectEntity.Name),
                                               ScanOperator.Equal,
                                               queryProperty);

            }

            var projectsToDelete = await dynamoDbContext.FromScanAsync<TEntity>(scanConfig)
                                                        .GetRemainingAsync(cancellationToken)
                                                        .ConfigureAwait(false);

            foreach (var project in projectsToDelete)
            {
                await dynamoDbContext.DeleteAsync(project, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
