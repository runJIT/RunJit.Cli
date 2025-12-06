using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.New;

namespace RunJit.Cli.New.NugetProject
{
    internal static class AddNewNugetProjectCommandBuilderExtension
    {
        internal static void AddNewNugetProjectCommandBuilder(this IServiceCollection services)
        {
            services.AddNugetProjectCodeGen();
            services.AddNewNugetProjectOptionsBuilder();
            services.AddNewNugetProjectService();

            services.AddSingletonIfNotExists<INewSubCommandBuilder, NewNugetProjectCommandBuilder>();
        }
    }

    internal sealed class NewNugetProjectCommandBuilder(NewNugetProjectService minimalApiProjectService,
                                                        NewNugetProjectOptionsBuilder optionsBuilder) : INewSubCommandBuilder
    {
        public Command Build()
        {
            var command = new Command("nuget-project", "The command to create a project for a nuget library, ready to deploy.");
            optionsBuilder.Build().ToList().ForEach(option => command.AddOption(option));

            command.Handler = CommandHandler.Create<bool, string, DirectoryInfo, int>((startIde,
                                                                                       projectName,
                                                                                       targetDirectory,
                                                                                       targetFramework) => minimalApiProjectService.HandleAsync(new NewNugetProjectParameters(startIde, projectName, targetDirectory,
                                                                                                                                                                              targetFramework)));

            return command;
        }
    }
}
