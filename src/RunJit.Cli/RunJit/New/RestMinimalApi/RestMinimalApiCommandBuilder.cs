using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.New.MinimalApiProject;
using RunJit.Cli.New.RestMinimalApi.Options;
using RunJit.Cli.RunJit.Check.Backend.Builds;
using RunJit.Cli.RunJit.New;

namespace RunJit.Cli.New.RestMinimalApi
{
    internal static class AddNewRestMinimalApiCommandBuilderExtension
    {
        internal static void AddNewRestMinimalApiCommandBuilder(this IServiceCollection services)
        {
            services.AddMinimalApiProjectCodeGen();
            services.AddNewRestMinimalApiOptionsBuilder();
            services.AddNewRestMinimalApiService();
            services.AddNewMinimalApiProjectArgumentsBuilder();

            services.AddSingletonIfNotExists<INewSubCommandBuilder, NewRestMinimalApiCommandBuilder>();
        }
    }

    internal sealed class NewRestMinimalApiCommandBuilder(NewRestMinimalApiService minimalApiProjectService,
                                                          NewRestMinimalApiOptionsBuilder optionsBuilder,
                                                          NewMinimalApiProjectArgumentsBuilder argumentsBuilder) : INewSubCommandBuilder
    {
        public Command Build()
        {
            var command = new Command("minimal-rest-api", "The command to create a rest api with all CRUD operations");
            optionsBuilder.Build().ToList().ForEach(option => command.AddOption(option));
            argumentsBuilder.Build().ToList().ForEach(argument => command.AddArgument(argument));

            command.Handler = CommandHandler.Create<string, string, string, string, int, string, string>((solutionFileOrGitRepos,
                                                                                                          basePath,
                                                                                                          workingDirectory,
                                                                                                          entity,
                                                                                                          version,
                                                                                                          queryProperty,
                                                                                                          domainName) => minimalApiProjectService.HandleAsync(new NewRestMinimalApiParameters(solutionFileOrGitRepos, 
                                                                                                                                                                                              basePath,
                                                                                                                                                                                              workingDirectory,
                                                                                                                                                                                              entity, version, queryProperty,
                                                                                                                                                                                              domainName)));

            return command;
        }
    }
}
