using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Migrate.Endpoints.RegisterEndpoints.Arguments
{
    internal static class AddRegisterEndpointsArgumentsBuilderExtension
    {
        internal static void AddRegisterEndpointsArgumentsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRegisterEndpointsArgumentsBuilder, RegisterEndpointsArgumentsBuilder>();
        }
    }

    internal interface IRegisterEndpointsArgumentsBuilder
    {
        IEnumerable<System.CommandLine.Argument> Build();
    }

    internal sealed class RegisterEndpointsArgumentsBuilder : IRegisterEndpointsArgumentsBuilder
    {
        public IEnumerable<System.CommandLine.Argument> Build()
        {
            // Not used at the moment; kept for parity with sample structure
            yield break;
        }
    }
}
