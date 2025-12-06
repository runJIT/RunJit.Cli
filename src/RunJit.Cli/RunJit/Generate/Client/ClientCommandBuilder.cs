using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Generate.Client
{
    internal static class AddClientCommandBuilderExtension
    {
        internal static void AddClientCommandBuilder(this IServiceCollection services,
                                                     IConfiguration configuration)
        {
            services.AddClient(configuration);
            services.AddClientOptionsBuilder();

            services.AddSingletonIfNotExists<IGenerateSubCommandBuilder, ClientCommandBuilder>();
        }
    }

    internal sealed class ClientCommandBuilder(IClientGen clientGen,
                                               IClientGenOptionsBuilder optionsBuilder) : IGenerateSubCommandBuilder
    {
        public Command Build()
        {
            var command = new Command("client", "The command to generate a new .net client into a .net web api project");
            optionsBuilder.Build().ToList().ForEach(option => command.AddOption(option));

            command.Handler = CommandHandler.Create<bool, bool, FileInfo, bool>((usevisualstudio,
                                                                                 build,
                                                                                 solution,
                                                                                 useOpenApiJson) => clientGen.HandleAsync(new ClientParameters(usevisualstudio, build, solution,
                                                                                                                                               useOpenApiJson)));

            return command;
        }
    }
}
