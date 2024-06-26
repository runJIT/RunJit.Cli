﻿using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;

namespace RunJit.Cli.RunJit.Generate
{
    public static class AddGenerateCommandBuilderExtension
    {
        public static void AddGenerateCommandBuilder(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddClientCommandBuilder(configuration);
            
            services.AddSingletonIfNotExists<IRunJitSubCommandBuilder, GenerateCommandBuilder>();
        }
    }

    internal sealed class GenerateCommandBuilder(IEnumerable<IGenerateSubCommandBuilder> generateSubCommandBuilders) : IRunJitSubCommandBuilder
    {
        public Command Build()
        {
            var generateCommand = new Command("generate", "The command to generate something like a client, tests or any other cool things");
            generateSubCommandBuilders.ToList().ForEach(builder => generateCommand.AddCommand(builder.Build()));
            return generateCommand;
        }
    }
}
