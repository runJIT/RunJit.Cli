using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Update;

namespace RunJit.Cli.Update.TargetPlatform
{
    internal static class AddUpdateTargetPlatformCommandBuilderExtension
    {
        internal static void AddUpdateTargetPlatformCommandBuilder(this IServiceCollection services,
                                                                   IConfiguration configuration)
        {
            services.AddUpdateTargetPlatformOptionsBuilder();
            services.AddUpdateTargetPlatformArgumentBuilder();
            services.AddUpdateTargetPlatformOptionsBuilder();

            services.AddSingletonIfNotExists<IUpdateSubCommandBuilder, UpdateTargetPlatformCommandBuilder>();
        }
    }

    internal sealed class UpdateTargetPlatformCommandBuilder(IUpdateTargetPlatform updateTargetPlatform,
                                                             IUpdateTargetPlatformArgumentBuilder argumentsBuilder,
                                                             IUpdateTargetPlatformOptionsBuilder optionsBuilder) : IUpdateSubCommandBuilder
    {
        public Command Build()
        {
            var command = new Command("platform", "Update the target platform of your application. Samples: win-x86;win-x64;win-arm;win-arm64;linux-x64;linux-arm;linux-arm64;linux-musl-x64;linux-musl-arm64;osx-x64;osx-arm64;freebsd-x64");
            optionsBuilder.Build().ToList().ForEach(option => command.AddOption(option));
            argumentsBuilder.Build().ToList().ForEach(argument => command.AddArgument(argument));

            command.Handler = CommandHandler.Create<string, string, string, string>((solutionFile,
                                                                                     gitRepos,
                                                                                     workingDirectory,
                                                                                     platform) => updateTargetPlatform.HandleAsync(new UpdateTargetPlatformParameters(solutionFile, gitRepos, workingDirectory,
                                                                                                                                                                      platform)));

            return command;
        }
    }
}
