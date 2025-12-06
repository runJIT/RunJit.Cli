using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Update;

namespace RunJit.Cli.Update
{
    internal static class AddDotNetCommandBuilderExtension
    {
        internal static void AddDotNetCommandBuilder(this IServiceCollection services)
        {
            services.AddUpdateDotNetVersionOptionsBuilder();
            services.AddDotNetArgumentsBuilder();
            services.AddUpdateDotNetVersion();

            services.AddSingletonIfNotExists<IUpdateSubCommandBuilder, DotNetCommandBuilder>();
        }
    }

    internal sealed class DotNetCommandBuilder(IUpdateDotNetVersion updateService,
                                               IDotNetArgumentsBuilder argumentsBuilder,
                                               IUpdateDotNetVersionOptionsBuilder optionsBuilder) : IUpdateSubCommandBuilder
    {
        public Command Build()
        {
            var command = new Command(".net", "Update the .Net version");
            optionsBuilder.Build().ToList().ForEach(option => command.AddOption(option));
            argumentsBuilder.Build().ToList().ForEach(argument => command.AddArgument(argument));

            command.Handler = CommandHandler.Create<string, string, string, int>((solutionFile,
                                                                                  gitRepos,
                                                                                  workingDirectory,
                                                                                  version) => updateService.HandleAsync(new UpdateDotNetVersionParameters(solutionFile, gitRepos, workingDirectory,
                                                                                                                                                          version)));

            return command;
        }
    }
}
