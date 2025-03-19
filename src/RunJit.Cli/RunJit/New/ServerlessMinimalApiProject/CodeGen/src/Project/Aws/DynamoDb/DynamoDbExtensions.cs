using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Extensions.Pack;

namespace $ProjectName$.Aws.DynamoDb
{
    internal static class DynamoDbExtensions
    {
        internal static Task DeleteAllAsync<TEntity>(this DynamoDBContext dynamoDbContext,
                                                     CancellationToken cancellationToken = default) where TEntity : class
        {
            return dynamoDbContext.DeleteAllAsync<TEntity>([], cancellationToken);
        }

        internal static async Task DeleteAllAsync<TEntity>(this DynamoDBContext dynamoDbContext,
                                                           (string PropertyName, DynamoDBEntry Value)[] queryProperties,
                                                           CancellationToken cancellationToken = default) where TEntity : class
        {
            var entitiesToDelete = await dynamoDbContext.GetAllAsync<TEntity>(queryProperties, cancellationToken).ConfigureAwait(false);

            foreach (var entity in entitiesToDelete)
            {
                await dynamoDbContext.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
            }
        }

        internal static Task<List<TEntity>> GetAllAsync<TEntity>(this DynamoDBContext dynamoDbContext,
                                                                 CancellationToken cancellationToken = default) where TEntity : class
        {
            return dynamoDbContext.GetAllAsync<TEntity>([], cancellationToken);
        }

        internal static async Task<List<TEntity>> GetAllAsync<TEntity>(this DynamoDBContext dynamoDbContext,
                                                                       (string PropertyName, DynamoDBEntry Value)[] queryProperties,
                                                                       CancellationToken cancellationToken = default) where TEntity : class
        {
            var scanConfig = new ScanOperationConfig { Filter = new ScanFilter() };

            foreach (var queryProperty in queryProperties)
            {
                if (queryProperty.Value.AsString().IsNull())
                {
                    continue;   
                }
                
                if (queryProperty.PropertyName.IsNotNullOrWhiteSpace())
                {
                    scanConfig.Filter.AddCondition(queryProperty.PropertyName,
                                                   ScanOperator.Equal,
                                                   queryProperty.Value);
                }
            }

            // Do not use .ToImmutableList(), because this objects will be mapped
            // to the domain models -> avoid another useless loop !
            var entities = await dynamoDbContext.FromScanAsync<TEntity>(scanConfig)
                                                .GetRemainingAsync(cancellationToken)
                                                .ConfigureAwait(false);

            return entities;
        }
    }
}
