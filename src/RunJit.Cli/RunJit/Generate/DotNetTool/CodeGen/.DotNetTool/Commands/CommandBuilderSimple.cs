using Argument.Check;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool.Models;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddCommandBuilderSimpleExtension
    {
        internal static void AddCommandBuilderSimple(this IServiceCollection services)
        {
            services.AddCommandHandlerBuilder();

            services.AddSingletonIfNotExists<CommandBuilderSimple>();
        }
    }

    internal sealed class CommandBuilderSimple(CommandHandlerBuilder commandHandlerBuilder)
    {
        private const string Template = @"
using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace $namespace$
{                
    internal static class Add$command-name$CommandBuilderExtension
    {
        internal static void Add$command-name$CommandBuilder(this IServiceCollection services, 
                                                             IConfiguration configuration)
        {
            services.Add$command-name$Handler(configuration);

            services.AddSingletonIfNotExists<$command-name$CommandBuilder>();
        }
    }

    internal sealed class $command-name$CommandBuilder($command-name$Handler $command-handler-argument-name$Handler)$interface$
    {   
        public Command Build()
        {
            var command = new Command(""$command-argument-name$"", ""$command-description$"");            
            command.Handler = $command-handler$;
            return command;
        }
    }
}";

        public string Build(string project,
                            CommandInfo commandInfo,
                            CommandInfo? parentCommandInfo,
                            string nameSpace)
        {
            Throw.IfNullOrWhiteSpace(project);
            Throw.IfNullOrWhiteSpace(nameSpace);

            var commandHandler = commandHandlerBuilder.Build(commandInfo);

            var interfaceImplementation = parentCommandInfo.IsNull() || commandInfo.EqualsTo(parentCommandInfo) ? string.Empty : $" : I{parentCommandInfo.NormalizedName}SubCommandBuilder";

            var newTemplate = Template.Replace("$command-name$", commandInfo.NormalizedName)
                                      .Replace("$command-description$", commandInfo.Description)
                                      .Replace("$command-argument-name$", commandInfo.Name.ToLower())
                                      .Replace("$command-handler-argument-name$", commandInfo.NormalizedName.FirstCharToLower())
                                      .Replace("$command-handler$", commandHandler)
                                      .Replace("$namespace$", nameSpace)
                                      .Replace("$project-name$", project)
                                      .Replace("$interface$", interfaceImplementation);

            return newTemplate.FormatSyntaxTree();
        }
    }
}
