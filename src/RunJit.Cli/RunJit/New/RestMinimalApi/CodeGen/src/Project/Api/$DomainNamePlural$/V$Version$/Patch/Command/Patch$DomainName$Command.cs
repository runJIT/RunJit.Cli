using System.Text.Json.Nodes;
using Extensions.Pack;
using Newtonsoft.Json;
using $ProjectName$.Aws.DynamoDb;
using $ProjectName$.Database.Projects;
using $ProjectName$.JsonSerializing;
using Siemens.AspNet.ErrorHandling.Contracts;


namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Patch
{
    internal static class AddPatch$DomainName$CommandExtension
    {
        internal static void AddPatch$DomainName$Command(this IServiceCollection services,
                                                    IConfiguration configuration)
        {
            services.AddAmazonDynamoDbClientFactory(configuration);
            services.AddPatch$DomainName$RequestValidator();
            services.AddPatch$DomainName$RequestMapper();
            services.Add$DomainName$EntityToPatch$DomainName$RequestMapper();
            services.Add$DomainName$EntityMapper();
            services.AddJsonDiffer();
            
            
            services.AddSingletonIfNotExists<Patch$DomainName$Command>();
        }
    }

    internal sealed class Patch$DomainName$Command(IAmazonDynamoDbClientFactory dynamoDbClientFactory,
                                              Patch$DomainName$RequestValidator patch$DomainName$RequestValidator,
                                              $DomainName$EntityToPatch$DomainName$RequestMapper $DomainNameLower$RequestMapper,
                                              $DomainName$EntityMapper $DomainNameLower$Mapper)
    {
        internal async Task<$DomainName$> ExecuteAsync(JsonObject patchRequest,
                                                  Guid $DomainNameLower$Id,
                                                  CancellationToken cancellationToken)
        {

            // 1. Create instance of dynamo db context
            using var dbContext = dynamoDbClientFactory.Create();

            // 2. Try to get $DomainNameLower$ by id
            var $DomainNameLower$Entity = await dbContext.LoadAsync<$DomainName$Entity>($DomainNameLower$Id, cancellationToken).ConfigureAwait(false);

            // 3. If $DomainNameLower$Entity does not exist we throw not found.
            //    Hint we decide for the moment not to switch to IResult
            //    to avoid exceptions
            if ($DomainNameLower$Entity.IsNull())
            {
                throw new NotFoundDetailsException("$DomainName$ with the requested id to patch does not exists",
                                                   $"$DomainName$ with the requested id: {$DomainNameLower$Id} to patch does not exists",
                                                   ("$DomainName$Id", $DomainNameLower$Id),
                                                   ("PatchValues", patchRequest));
            }

            // 3. Map to domain model
            var patch$DomainName$Request = $DomainNameLower$RequestMapper.MapFrom($DomainNameLower$Entity);

            // 4. Apply requested changes 
            JsonConvert.PopulateObject(patchRequest.ToString(), patch$DomainName$Request);

            // 5. We have to validate the applied changes
            await patch$DomainName$RequestValidator.ValidateAsync(patchRequest, patch$DomainName$Request, $DomainNameLower$Id).ConfigureAwait(false);

            // 6. Save updated/patched $DomainNameLower$
            await dbContext.SaveAsync($DomainNameLower$Entity, cancellationToken).ConfigureAwait(false);

            // 7. Mapping the entity to the domain / api model
            var $DomainNameLower$ = $DomainNameLower$Mapper.MapFrom($DomainNameLower$Entity);
            
            // 7. Return the whole $DomainNameLower$
            return $DomainNameLower$;
        }
    }
}
