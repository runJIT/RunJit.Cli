using Argument.Check;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool.Models;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddCommandServiceBuilderExtension
    {
        internal static void AddCommandServiceBuilder(this IServiceCollection services)
        {
            services.AddCommandMethodBuilderBuilder();

            services.AddSingletonIfNotExists<CommandServiceBuilder>();
        }
    }

    internal sealed class CommandServiceBuilder(CommandMethodBuilder commandMethodBuilder)
    {
        private const string Template = """
                                        using Extensions.Pack;
                                        using Microsoft.Extensions.Configuration;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {    
                                            internal static class Add$command-name$HandlerExtension
                                            {
                                                internal static void Add$command-name$Handler(this IServiceCollection services, 
                                                                                              IConfiguration configuration)
                                                {
                                                    services.AddOutputService();
                                                    services.Add$dotNetToolName$HttpClientFactory(configuration);
                                                
                                                    services.AddSingletonIfNotExists<$command-name$Handler>();
                                                }
                                            }

                                            internal sealed class $command-name$Handler(OutputService outputService$dependencies$)
                                            {       
                                                public async Task HandleAsync($command-name$Parameters parameters, CancellationToken cancellationToken = default)
                                                {
                                                    $methodBody$
                                                }
                                            }
                                        }
                                        """;

        public string Build(string project,
                            CommandInfo commandInfo,
                            string nameSpace,
                            DotNetToolInfos dotNetToolName)
        {
            Throw.IfNullOrWhiteSpace(project);
            Throw.IfNullOrWhiteSpace(nameSpace);

            var dependencies = commandInfo.EndpointInfo.IsNull() ? string.Empty : $", {dotNetToolName.NormalizedName}HttpClientFactory {dotNetToolName.NormalizedName.FirstCharToLower()}HttpClientFactory";
            var methodBody = commandInfo.EndpointInfo.IsNull() ? "throw new NotImplementedException();" : commandMethodBuilder.BuildFor(commandInfo.EndpointInfo, dotNetToolName);

            var templateToUse = commandInfo.CodeTemplate.IsNullOrWhiteSpace() ? Template : commandInfo.CodeTemplate;

            var newTemplate = templateToUse.Replace("$command-name$", commandInfo.NormalizedName)
                                           .Replace("$namespace$", nameSpace)
                                           .Replace("$project-name$", project)
                                           .Replace("$methodBody$", methodBody)
                                           .Replace("$dependencies$", dependencies)
                                           .Replace("$dotNetToolName$", dotNetToolName.NormalizedName);

            if (commandInfo.NoSyntaxTreeFormatting)
            {
                return newTemplate;
            }

            return newTemplate.FormatSyntaxTree();
        }
    }
}
