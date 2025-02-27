using Extensions.Pack;
using $ProjectName$.Validations;

namespace $ProjectName$.Api.$DomainName$s.V$Version$.Create
{
    public static class AddUpdate$DomainName$RequestValidatorExtension
    {
        internal static void AddUpdate$DomainName$RequestValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<Update$DomainName$RequestValidator>();
        }
    }

    internal sealed class Update$DomainName$RequestValidator : IRequestValidator<Create$DomainName$Request>
    {
        public Task ValidateAsync(Create$DomainName$Request request)
        {
            return Task.CompletedTask;
        }

        public Task ValidateAsync(Create$DomainName$Request request,
                                  Guid id)
        {
            return ValidateAsync(request);
        }
    }
}
