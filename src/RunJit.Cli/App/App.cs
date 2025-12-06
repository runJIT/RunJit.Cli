using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.RunJit;

namespace RunJit.Cli
{
    internal sealed class App(IServiceProvider serviceProvider)
    {
        public async Task<int> RunAsync(string[] args)
        {
            // Get needed service to invoke client generator. Important anything have to be handled by dependency injection
            var rootCommand = serviceProvider.GetRequiredService<RunJitCommandBuilder>().Build();
            var errorHandler = serviceProvider.GetRequiredService<ErrorHandler>();
            var dotNetCliArgumentFixer = serviceProvider.GetRequiredService<RunJitArgumentFixer>();

            // Setup command line builder from microsoft cli sdk
            var commandLineBuilder = new CommandLineBuilder(rootCommand);
            commandLineBuilder.UseMiddleware(errorHandler.HandleErrorsAsync);
            commandLineBuilder.UseDefaults();
            var parser = commandLineBuilder.Build();

            // We automatically add a version command
            var option = parser.Configuration.RootCommand.Options.Single(o => o.Name.EqualsTo("version")).As<Option?>();
            option?.AddAlias("-v");

            // Fix or update command parameter
            var fixedArgs = dotNetCliArgumentFixer.Fix(args);

            // Here the cli sdk of microsoft will be invoked and manage any command execution
            var result = await parser.InvokeAsync(fixedArgs).ConfigureAwait(false);

            return result;
        }
    }
}
