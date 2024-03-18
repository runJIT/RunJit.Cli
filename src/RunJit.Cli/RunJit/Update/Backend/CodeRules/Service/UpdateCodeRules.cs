﻿using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;

namespace RunJit.Cli.RunJit.Update.Backend.CodeRules
{
    public static class AddUpdateCodeRulesExtension
    {
        public static void AddUpdateCodeRules(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddUpdateCodeRulesParameters();

            services.AddUpdateLocalSolutionFile();
            services.AddCloneReposAndUpdateAll();

            services.AddSingletonIfNotExists<IUpdateCodeRules, UpdateCodeRules>();
        }
    }

    internal interface IUpdateCodeRules
    {
        Task HandleAsync(UpdateCodeRulesParameters parameters);
    }

    internal class UpdateCodeRules(IEnumerable<IUpdateCodeRulesStrategy> updateCodeRulesStrategies) : IUpdateCodeRules
    {
        public Task HandleAsync(UpdateCodeRulesParameters parameters)
        {
            var updateCodeRulesStrategy = updateCodeRulesStrategies.Where(x => x.CanHandle(parameters)).ToImmutableList();
            if (updateCodeRulesStrategy.Count < 1)
            {
                throw new RunJitException($"Could not find a strategy a update nuget strategy for parameters: {parameters}");
            }

            if (updateCodeRulesStrategy.Count > 1)
            {
                throw new RunJitException($"Found more than one strategy a update nuget strategy for parameters: {parameters}");
            }

            return updateCodeRulesStrategy[0].HandleAsync(parameters);
        }
    }


    interface IUpdateCodeRulesStrategy
    {
        bool CanHandle(UpdateCodeRulesParameters parameters);

        Task HandleAsync(UpdateCodeRulesParameters parameters);
    }
}