using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Migrate.Endpoints.RegisterEndpoints.Arguments;
using RunJit.Cli.Migrate.Endpoints.RegisterEndpoints.Options;
using RunJit.Cli.Migrate.Endpoints.RegisterEndpoints.Service;

namespace RunJit.Cli.Migrate.Endpoints.RegisterEndpoints
{
    internal static class AddRegisterEndpointsCommandBuilderExtension
    {
        internal static void AddRegisterEndpointsCommandBuilder(this IServiceCollection services)
        {
            services.AddRegisterEndpointsOptionsBuilder();
            services.AddRegisterEndpointsArgumentsBuilder();
            services.AddRegisterEndpoints();

            services.AddSingletonIfNotExists<IEndpointsSubCommandBuilder, RegisterEndpointsCommandBuilder>();
        }
    }

    internal sealed class RegisterEndpointsCommandBuilder(IRegisterEndpointsOptionsBuilder optionsBuilder,
                                                          IRegisterEndpoints handler) : IEndpointsSubCommandBuilder
    {
        public Command Build()
        {
            var command = new Command("register-endpoints", "Register endpoints in Startup files (Add*/Map* methods) for minimal APIs.");

            // options
            optionsBuilder.Build().ForEach(opt => command.AddOption(opt));

            command.Handler = CommandHandler.Create<string, string, string>((solution,
                                                                             webApiProject,
                                                                             domainNamePlural) => handler.HandleAsync(new RegisterEndpointsParameters(solution ?? string.Empty,
                                                                                                                                                      webApiProject ?? string.Empty,
                                                                                                                                                      domainNamePlural ?? string.Empty)));

            return command;
        }
    }
}
