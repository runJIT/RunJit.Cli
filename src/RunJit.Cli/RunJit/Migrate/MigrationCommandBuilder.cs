using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Migrate.Endpoints;
using RunJit.Cli.RunJit;

namespace RunJit.Cli.Migrate
{
    internal static class AddMigrationCommandBuilderExtension
    {
        internal static void AddMigrationCommandBuilder(this IServiceCollection services)
        {
            services.AddEndpointsCommandBuilder();

            services.AddSingletonIfNotExists<IRunJitSubCommandBuilder, MigrationCommandBuilder>();
        }
    }

    internal sealed class MigrationCommandBuilder(IEnumerable<IMigrationSubCommandBuilder> migrationSubCommandBuilders)
        : IRunJitSubCommandBuilder
    {
        public Command Build()
        {
            var migrationCommand = new Command("migrate", "Migration utilities and helpers.");
            migrationSubCommandBuilders.ToList().ForEach(builder => migrationCommand.AddCommand(builder.Build()));

            return migrationCommand;
        }
    }
}
