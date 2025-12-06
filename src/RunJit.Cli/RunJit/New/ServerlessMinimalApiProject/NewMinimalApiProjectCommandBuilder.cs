using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.New;

namespace RunJit.Cli.New.MinimalApiProject
{
    internal static class AddNewMinimalApiProjectCommandBuilderExtension
    {
        internal static void AddNewMinimalApiProjectCommandBuilder(this IServiceCollection services)
        {
            services.AddMinimalApiProjectCodeGen();
            services.AddNewMinimalApiProjectOptionsBuilder();
            services.AddNewMinimalApiProjectService();

            services.AddSingletonIfNotExists<INewSubCommandBuilder, NewMinimalApiProjectCommandBuilder>();
        }
    }

    internal sealed class NewMinimalApiProjectCommandBuilder(NewMinimalApiProjectService minimalApiProjectService,
                                                             NewMinimalApiProjectOptionsBuilder optionsBuilder) : INewSubCommandBuilder
    {
        public Command Build()
        {
            var command = new Command("minimal-api-serverless", "The command to create a new basic minimal api project.");
            optionsBuilder.Build().ToList().ForEach(option => command.AddOption(option));

            command.Handler = CommandHandler.Create<bool, bool, string, string, DirectoryInfo, int>((usevisualstudio,
                                                                                                     build,
                                                                                                     projectName,
                                                                                                     basePath,
                                                                                                     targetDirectory,
                                                                                                     targetFramework) => minimalApiProjectService.HandleAsync(new NewMinimalApiProjectParameters(usevisualstudio, build, projectName,
                                                                                                                                                                                                 basePath, targetDirectory, targetFramework)));

            return command;
        }
    }
}
