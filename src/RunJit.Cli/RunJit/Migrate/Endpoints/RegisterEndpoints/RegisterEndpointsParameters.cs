using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Migrate.Endpoints.RegisterEndpoints
{
    internal static class AddRegisterEndpointsParametersExtension
    {
        internal static void AddRegisterEndpointsParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<RegisterEndpointsParameters>();
        }
    }

    internal sealed record RegisterEndpointsParameters(string SolutionFile,
                                                       string WebApiProject,
                                                       string DomainNamePlural);
}
