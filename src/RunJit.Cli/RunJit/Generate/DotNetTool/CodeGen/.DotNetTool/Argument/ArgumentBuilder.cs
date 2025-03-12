using Argument.Check;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool.Models;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddArgumentBuilderCodeExtension
    {
        internal static void AddArgumentBuilderCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ArgumentBuilder>();
        }
    }

    internal sealed class ArgumentBuilder
    {
        private const string Template =
            @"
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace $namespace$
{    
    internal static class Add$command-name$ArgumentBuilderExtension
    {
        internal static void Add$command-name$ArgumentBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<$command-name$ArgumentBuilder>();
        }
    }

    internal sealed class $command-name$ArgumentBuilder
    {                                        
        internal System.CommandLine.Argument Build()
        {
            var argument = new System.CommandLine.Argument<$type$>()
            {
                Name = ""$argument-name$"",
                Description = ""$argument-description$""
            };
            
            return argument;
        }
    }
}";

        public string Build(string projectName,
                            CommandInfo parameterInfo,
                            string nameSpace)
        {
            Throw.IfNullOrWhiteSpace(projectName);
            Throw.IfNull(() => parameterInfo);
            Throw.IfNullOrWhiteSpace(nameSpace);

            var newTemplate = Template.Replace("$project-name$", projectName)
                                      .Replace("$command-name$", parameterInfo.NormalizedName)
                                      .Replace("$argument-name$", parameterInfo.Argument?.Name)
                                      .Replace("$namespace$", nameSpace)
                                      .Replace("$type$", parameterInfo.Argument?.OptimizedType)
                                      .Replace("$argument-description$", parameterInfo.Argument?.Description);

            return newTemplate.FormatSyntaxTree();
        }
    }
}
