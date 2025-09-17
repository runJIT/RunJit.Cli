using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Migrate.Endpoints.RegisterEndpoints;

namespace RunJit.Cli.Migrate.Endpoints
{
    internal static class AddEndpointsCommandBuilderExtension
    {
        internal static void AddEndpointsCommandBuilder(this IServiceCollection services)
        {
            // child commands
            services.AddRegisterEndpointsCommandBuilder();

            // register this node as a migration sub command
            services.AddSingletonIfNotExists<IMigrationSubCommandBuilder, EndpointsCommandBuilder>();
        }
    }

    internal sealed class EndpointsCommandBuilder(IEnumerable<IEndpointsSubCommandBuilder> endpointsSubCommandBuilders)
        : IMigrationSubCommandBuilder
    {
        public Command Build()
        {
            var endpointsCommand = new Command("endpoints", "Endpoints migration helpers.");
            endpointsSubCommandBuilders.ToList().ForEach(builder => endpointsCommand.AddCommand(builder.Build()));
            return endpointsCommand;
        }
    }
}
