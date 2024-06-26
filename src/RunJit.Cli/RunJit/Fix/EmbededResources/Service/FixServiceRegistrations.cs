﻿using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;

namespace RunJit.Cli.RunJit.Fix.EmbededResources
{
    public static class AddFixEmbeddedResourcesExtension
    {
        public static void AddFixEmbeddedResources(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddFixEmbeddedResourcesParameters();

            services.AddUpdateLocalSolutionFile();
            services.AddCloneReposAndUpdateAll();

            services.AddSingletonIfNotExists<IFixEmbeddedResources, FixEmbeddedResources>();
        }
    }

    internal interface IFixEmbeddedResources
    {
        Task HandleAsync(FixEmbeddedResourcesParameters parameters);
    }

    internal class FixEmbeddedResources(IEnumerable<IFixEmbeddedResourcesStrategy> fixServiceRegistrationsStrategies) : IFixEmbeddedResources
    {
        public Task HandleAsync(FixEmbeddedResourcesParameters parameters)
        {
            var fixServiceRegistrationsStrategy = fixServiceRegistrationsStrategies.Where(x => x.CanHandle(parameters)).ToImmutableList();
            if (fixServiceRegistrationsStrategy.Count < 1)
            {
                throw new RunJitException($"Could not find a strategy a update nuget strategy for parameters: {parameters}");
            }

            if (fixServiceRegistrationsStrategy.Count > 1)
            {
                throw new RunJitException($"Found more than one strategy a update nuget strategy for parameters: {parameters}");
            }

            return fixServiceRegistrationsStrategy[0].HandleAsync(parameters);
        }
    }


    interface IFixEmbeddedResourcesStrategy
    {
        bool CanHandle(FixEmbeddedResourcesParameters parameters);

        Task HandleAsync(FixEmbeddedResourcesParameters parameters);
    }
}
