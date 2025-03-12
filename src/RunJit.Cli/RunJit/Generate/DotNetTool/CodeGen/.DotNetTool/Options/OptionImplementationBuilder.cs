using Argument.Check;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool.Models;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddOptionImplementationBuilderExtension
    {
        internal static void AddOptionImplementationBuilder(this IServiceCollection services)
        {
            services.AddOptionMethodsBuilder();

            services.AddSingletonIfNotExists<OptionImplementationBuilder>();
        }
    }

    internal sealed class OptionImplementationBuilder
    {
        private const string Template =
            @"using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace $namespace$
{    

    internal static class Add$command-name$OptionsBuilderExtension
    {
        internal static void Add$command-name$OptionsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<$command-name$OptionsBuilder>();
        }
    }

    internal sealed class $command-name$OptionsBuilder
    {
        internal IEnumerable<Option> Build()
        {   
$yield-option$
        }

$build-option-method$                                       
    }
}";

        private readonly OptionMethodsBuilder _optionMethodsBuilder;

        public OptionImplementationBuilder(OptionMethodsBuilder optionMethodsBuilder)
        {
            Throw.IfNull(() => optionMethodsBuilder);

            _optionMethodsBuilder = optionMethodsBuilder;
        }

        public string Build(string projectName,
                            CommandInfo parameterInfo,
                            string nameSpace)
        {
            Throw.IfNullOrWhiteSpace(projectName);
            Throw.IfNull(() => parameterInfo);
            Throw.IfNullOrWhiteSpace(nameSpace);

            var optionsMethods = _optionMethodsBuilder.Build(parameterInfo.Options).ToList();
            var optionsMethodAsString = optionsMethods.Select(m => m.MethodSyntax).Flatten(Environment.NewLine);
            var yieldStatements = optionsMethods.Select(m => $"            yield return {m.MethodName}();").Flatten(Environment.NewLine);

            var newTemplate = Template.Replace("$project-name$", projectName)
                                      .Replace("$command-name$", parameterInfo.NormalizedName)
                                      .Replace("$yield-option$", yieldStatements)
                                      .Replace("$namespace$", nameSpace)
                                      .Replace("$build-option-method$", optionsMethodAsString);

            return newTemplate.FormatSyntaxTree();
        }
    }
}
