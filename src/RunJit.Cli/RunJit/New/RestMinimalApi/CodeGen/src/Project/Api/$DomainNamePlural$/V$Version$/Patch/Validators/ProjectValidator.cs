using Extensions.Pack;

namespace $ProjectName$.Api.$DomainNamePlural$.V$Version$.Patch
{
    public static class Add$DomainName$ValidatorValidatorExtension
    {
        internal static void Add$DomainName$ValidatorValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<$DomainName$Validator>();
        }
    }
    
    internal sealed class $DomainName$Validator
    {
        public Task ValidateAsync($DomainName$ request)
        {
            return Task.CompletedTask;
        }
    }
}
