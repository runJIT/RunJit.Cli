using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Extensions.Pack;

namespace $ProjectName$.Aws.DynamoDb
{
    internal static class DynamoDbExtensions
    {
        internal static async Task DeleteAllAsync<TEntity>(this DynamoDBContext dynamoDbContext,
                                                           string queryProperty = "",
                                                           CancellationToken cancellationToken = default) where TEntity : class
        {
            var scanConfig = new ScanOperationConfig
            {
                Filter = new ScanFilter()
            };

            if (queryProperty.IsNotNullOrWhiteSpace())
            {
                scanConfig.Filter.AddCondition(queryProperty,
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

        internal static async Task<List<TEntity>> GetAllAsync<TEntity>(this DynamoDBContext dynamoDbContext,
                                                        string queryProperty = "",
                                                        CancellationToken cancellationToken = default) where TEntity : class
        {
            var scanConfig = new ScanOperationConfig
            {
                Filter = new ScanFilter()
            };

            if (queryProperty.IsNotNullOrWhiteSpace())
            {
                scanConfig.Filter.AddCondition(queryProperty,
                                               ScanOperator.Equal,
                                               queryProperty);
            }


            // Do not use .ToImmutableList(), because this objects will be mapped
            // to the domain models -> avoid another useless loop !
            var projectsToDelete = await dynamoDbContext.FromScanAsync<TEntity>(scanConfig)
                                                        .GetRemainingAsync(cancellationToken)
                                                        .ConfigureAwait(false);

            return projectsToDelete;
        }
    }
}
