using Argument.Check;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool.Models;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddCommandBuilderForSubCommandsExtension
    {
        internal static void AddCommandBuilderForSubCommands(this IServiceCollection services)
        {
            services.AddCommandHandlerBuilder();

            services.AddSingletonIfNotExists<CommandBuilderForSubCommands>();
        }
    }

    internal sealed class CommandBuilderForSubCommands
    {
        private const string Template = @"
using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
$usings$

namespace $namespace$
{       
    internal static class Add$command-name$CommandBuilderExtension
    {
        internal static void Add$command-name$CommandBuilder(this IServiceCollection services, IConfiguration configuration)
        {
            $subCommandRegistration$

            services.AddSingletonIfNotExists<$commandRegistration$>();
        }
    }

    internal sealed class $command-name$CommandBuilder(IEnumerable<I$command-name$SubCommandBuilder> $command-argument-name$SubCommandBuilders)$interface$
    {     
        public Command Build()
        {
            var $command-argument-name$Command = new Command(""$command-argument-name$"", ""$command-description$"");
            $command-argument-name$SubCommandBuilders.ToList().ForEach(builder => $command-argument-name$Command.AddCommand(builder.Build()));            
            return $command-argument-name$Command;
        }
    }
}";

        private readonly CommandHandlerBuilder _commandHandlerBuilder;

        public CommandBuilderForSubCommands(CommandHandlerBuilder commandHandlerBuilder)
        {
            Throw.IfNull(() => commandHandlerBuilder);

            _commandHandlerBuilder = commandHandlerBuilder;
        }

        public string Build(string project,
                            CommandInfo commandInfo,
                            CommandInfo? parentCommandInfo,
                            string nameSpace)
        {
            Throw.IfNullOrWhiteSpace(project);
            Throw.IfNullOrWhiteSpace(nameSpace);

            var commandHandler = _commandHandlerBuilder.Build(commandInfo);

            var interfaceImplementation = parentCommandInfo.IsNull() || commandInfo.EqualsTo(parentCommandInfo) ? string.Empty : $" : I{parentCommandInfo.NormalizedName}SubCommandBuilder";

            var subCommandRegistration = commandInfo.SubCommands.Select(command => $"services.Add{command.NormalizedName}CommandBuilder(configuration);").ToFlattenString(Environment.NewLine);
            var subCommandUsings = commandInfo.SubCommands.Select(command => $"using {nameSpace}.{command.NormalizedName};").ToFlattenString(Environment.NewLine);
            var commandRegistration = parentCommandInfo.IsNull() || commandInfo.EqualsTo(parentCommandInfo) ? $"{commandInfo.NormalizedName}CommandBuilder" : $"I{parentCommandInfo.NormalizedName}SubCommandBuilder, {commandInfo.NormalizedName}CommandBuilder";

            var newTemplate = Template.Replace("$command-name$", commandInfo.NormalizedName)
                                      .Replace("$command-argument-name$", commandInfo.NormalizedName.ToLowerInvariant())
                                      .Replace("$namespace$", nameSpace)
                                      .Replace("$command-description$", commandInfo.Description)
                                      .Replace("$command-handler$", commandHandler)
                                      .Replace("$project-name$", project)
                                      .Replace("$interface$", interfaceImplementation)
                                      .Replace("$subCommandRegistration$", subCommandRegistration)
                                      .Replace("$usings$", subCommandUsings)
                                      .Replace("$commandRegistration$", commandRegistration);

            return newTemplate.FormatSyntaxTree();
        }
    }
}
