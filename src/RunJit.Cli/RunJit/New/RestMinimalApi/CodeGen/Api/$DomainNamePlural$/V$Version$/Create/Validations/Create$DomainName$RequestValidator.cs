using Extensions.Pack;

namespace $ProjectName$.Api.Projects.V$Version$.Create
{
    public static class AddCreate$DomainModelName$RequestValidatorExtension
    {
        internal static void AddCreate$DomainModelName$RequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Create$DomainModelName$RequestValidator>();
        }
    }

    internal sealed class Create$DomainModelName$RequestValidator
    {
        public Task ValidateAsync(Create$DomainModelName$Request request)
        {
            Console.WriteLine(request);
            return Task.CompletedTask;
        }
    }
}
