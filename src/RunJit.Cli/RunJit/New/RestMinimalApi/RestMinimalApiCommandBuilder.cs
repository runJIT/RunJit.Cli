using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.New.MinimalApiProject;
using RunJit.Cli.New.RestMinimalApi.Options;
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

            services.AddSingletonIfNotExists<INewSubCommandBuilder, NewRestMinimalApiCommandBuilder>();
        }
    }

    internal sealed class NewRestMinimalApiCommandBuilder(NewRestMinimalApiService minimalApiProjectService,
                                                          NewRestMinimalApiOptionsBuilder optionsBuilder) : INewSubCommandBuilder
    {
        public Command Build()
        {
            var command = new Command("minimal-rest-api", "The command to create a rest api with all CRUD operations");
            optionsBuilder.Build().ToList().ForEach(option => command.AddOption(option));

            command.Handler = CommandHandler.Create<FileInfo, string, string, string, int, string>((solution, gitRepos, workingDirectory, domainModel, version, filterProperty) => minimalApiProjectService.HandleAsync(new NewRestMinimalApiParameters(solution, gitRepos, workingDirectory, domainModel, version, filterProperty)));

            return command;
        }
    }
}
