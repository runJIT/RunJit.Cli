using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Fix;

namespace RunJit.Cli.Fix.ProjectSettings
{
    internal static class AddFixProjectSettingsCommandBuilderExtension
    {
        internal static void AddFixProjectSettingsCommandBuilder(this IServiceCollection services)
        {
            services.AddFixProjectSettingsOptionsBuilder();

            // services.AddFixProjectSettingsArgumentsBuilder();
            services.AddFixProjectSettings();

            services.AddSingletonIfNotExists<IFixSubCommandBuilder, FixProjectSettingsCommandBuilder>();
        }
    }

    internal sealed class FixProjectSettingsCommandBuilder(IFixProjectSettings updateService,

                                                           // IFixProjectSettingsArgumentsBuilder argumentsBuilder,
                                                           IFixProjectSettingsOptionsBuilder optionsBuilder) : IFixSubCommandBuilder
    {
        public Command Build()
        {
            var command = new Command("projects", "Detects all service and options usage and fix their registrations");
            optionsBuilder.Build().ToList().ForEach(option => command.AddOption(option));

            // argumentsBuilder.Build().ToList().ForEach(argument => command.AddArgument(argument));
            command.Handler = CommandHandler.Create<string, string, string, string>((solution,
                                                                                     gitRepos,
                                                                                     workingDirectory,
                                                                                     ignorePackages) => updateService.HandleAsync(new FixProjectSettingsParameters(solution ?? string.Empty, gitRepos ?? string.Empty, workingDirectory ?? string.Empty,
                                                                                                                                                                   ignorePackages ?? string.Empty)));

            return command;
        }
    }
}
