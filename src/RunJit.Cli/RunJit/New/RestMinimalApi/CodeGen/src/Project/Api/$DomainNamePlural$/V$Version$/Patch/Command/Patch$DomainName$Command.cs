using Extensions.Pack;
using Microsoft.AspNetCore.JsonPatch;
using $ProjectName$.Aws.DynamoDb;
using $ProjectName$.Database.Projects;
using $ProjectName$.Mapping;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Patch
{
    internal static class AddPatch$DomainName$CommandExtension
    {
        internal static void AddPatch$DomainName$Command(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddSingletonIfNotExists<Patch$DomainName$Command>();
        }
    }

    internal sealed class Patch$DomainName$Command(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                              $DomainName$Validator $DomainNameLower$EntityValidator,
                                              IMapper<$DomainName$Entity, $DomainName$> mapper)
    {
        internal async Task<$DomainName$> ExecuteAsync(JsonPatchDocument<$DomainName$> patchRequest, 
                                                  Guid $DomainNameLower$Id,
                                                  CancellationToken cancellationToken)
        {
            // 1. Create instance of dynamo db context
            using var dbContext = dynamoDbClientFactory.Create();

            // 2. Try to get $DomainNameLower$ by id
            var $DomainNameLower$Entity = await dbContext.LoadAsync<$DomainName$Entity>($DomainNameLower$Id, cancellationToken).ConfigureAwait(false);
            if ($DomainNameLower$Entity.IsNull())
            {
                throw new NotFoundDetailsException("$DomainName$ with the requested id to patch does not exists",
                                                   $"$DomainName$ with the requested id: {$DomainNameLower$Id} to patch does not exists",
                                                   ("$DomainName$Id", $DomainNameLower$Id),
                                                   ("PatchValues", patchRequest));
            }
            
            
            // 3. Map to domain model
            var $DomainNameLower$ = mapper.MapTo($DomainNameLower$Entity);
            
            // 4. Apply requested changes 
            patchRequest.ApplyTo($DomainNameLower$);
            
            // 5. We have to validate the applied changes
            await $DomainNameLower$EntityValidator.ValidateAsync($DomainNameLower$).ConfigureAwait(false);
            
            // 6. Save updated/patched $DomainNameLower$
            await dbContext.SaveAsync($DomainNameLower$Entity, cancellationToken).ConfigureAwait(false);

            // 7. Return the whole $DomainNameLower$
            return $DomainNameLower$;
        }
    }
}
