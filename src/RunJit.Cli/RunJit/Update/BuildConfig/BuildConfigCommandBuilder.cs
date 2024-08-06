﻿using System.CommandLine;
using System.CommandLine.Invocation;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Update.BuildConfig;

namespace RunJit.Cli.RunJit.Update.UpdateBuildConfig
{
    public static class AddUpdateBuildConfigCommandBuilderExtension
    {
        public static void AddUpdateBuildConfigCommandBuilder(this IServiceCollection services)
        {
            services.AddUpdateBuildConfigOptionsBuilder();
            services.AddUpdateBuildConfigArgumentsBuilder();
            services.AddUpdateBuildConfigService();
            services.AddUpdateBuildConfig();

            services.AddSingletonIfNotExists<IUpdateSubCommandBuilder, UpdateBuildConfigCommandBuilder>();
        }
    }
    
    internal class UpdateBuildConfigCommandBuilder(IUpdateBuildConfigService updateService,
                                             IUpdateBuildConfigArgumentsBuilder argumentsBuilder,
                                             IUpdateBuildConfigOptionsBuilder optionsBuilder) : IUpdateSubCommandBuilder
    {
        public Command Build()
        {
            var command = new Command("buildconfig", "Update all MsBuild bounded context like Directory.Build.props or Directory.Build.targets");
            optionsBuilder.Build().ToList().ForEach(option => command.AddOption(option));
            argumentsBuilder.Build().ToList().ForEach(argument => command.AddArgument(argument));

            command.Handler = CommandHandler.Create<string>((solutionFile) => updateService.HandleAsync(new UpdateBuildConfigParameters(solutionFile)));

            return command;
        }
    }
}
