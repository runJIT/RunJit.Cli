using Extensions.Pack;
using $ProjectName$.Aws.DynamoDb;
using $ProjectName$.Database.$DomainNamePlural$;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$
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
        internal async Task<$DomainName$> ExecuteAsync(Guid $IdUrlName$, 
                                                       CancellationToken cancellationToken)
        {
            using var dbContext = dynamoDbClientFactory.Create();

            var $DomainNameLower$Entity = await dbContext.LoadAsync<$DomainName$Entity>($IdUrlName$, cancellationToken).ConfigureAwait(false);
            
            if ($DomainNameLower$Entity.IsNull())
            {
                throw new NotFoundDetailsException("$DomainName$ not found", 
                                                   $"The requested User with the id: {$IdUrlName$} was not found.", 
                                                   ("$IdPropertyName$", $IdUrlName$));    
            }
            
            var $DomainNameLower$ = mapper.MapFrom($DomainNameLower$Entity);

            return $DomainNameLower$;
        }
    }
}
