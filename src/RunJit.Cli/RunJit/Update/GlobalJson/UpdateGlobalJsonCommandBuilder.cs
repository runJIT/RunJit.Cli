using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Update;

namespace RunJit.Cli.Update.GlobalJson
{
    internal static class AddUpdateGlobalJsonCommandBuilderExtension
    {
        internal static void AddUpdateGlobalJsonCommandBuilder(this IServiceCollection services)
        {
            services.AddUpdateGlobalJsonArgumentsBuilder();
            services.AddUpdateGlobalJsonBuildsOptionsBuilder();
            services.AddUpdateGlobalJsonService();

            services.AddSingletonIfNotExists<IUpdateSubCommandBuilder, UpdateGlobalJsonCommandBuilder>();
        }
    }

    internal sealed class UpdateGlobalJsonCommandBuilder(UpdateGlobalJsonBuildsOptionsBuilder checkBackendBuildsOptionsBuilder,
                                                         UpdateGlobalJsonService checkBackendBuilds) : IUpdateSubCommandBuilder
    {
        public Command Build()
        {
            var checkCommand = new Command("globaljson", "The command to check that all backends are buildable. Why we need it. Cause if new .Net updates comes out it could be new analyzer finds issues which do not before.");

            //checkBackendBuildsArgumentsBuilder.Build().ForEach(arg => checkCommand.AddArgument(arg));
            checkBackendBuildsOptionsBuilder.Build().ForEach(opt => checkCommand.AddOption(opt));

            checkCommand.Handler = CommandHandler.Create<string, string, string, string>((solution,
                                                                                          gitRepos,
                                                                                          workingDirectory,
                                                                                          ignorePackages) => checkBackendBuilds.HandleAsync(new UpdateGlobalJsonParameters(solution ?? string.Empty, gitRepos ?? string.Empty, workingDirectory ?? string.Empty,
                                                                                                                                                                           ignorePackages ?? string.Empty)));

            return checkCommand;
        }
    }
}
