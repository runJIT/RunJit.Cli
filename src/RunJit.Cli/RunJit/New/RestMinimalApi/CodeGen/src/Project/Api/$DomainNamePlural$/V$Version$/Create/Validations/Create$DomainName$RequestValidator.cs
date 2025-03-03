using Extensions.Pack;

namespace $ProjectName$.Api.Projects.V$Version$.Create
{
    public static class AddCreate$DomainName$RequestValidatorExtension
    {
        internal static void AddCreate$DomainName$RequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Create$DomainName$RequestValidator>();
        }
    }

    internal sealed class Create$DomainName$RequestValidator
    {
        public Task ValidateAsync(Create$DomainName$Request request)
        {
            Console.WriteLine(request);
            return Task.CompletedTask;
        }
    }
}
