using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;

namespace RunJit.Cli.Update.GlobalJson
{
    internal static class AddUpdateGlobalJsonServiceExtension
    {
        internal static void AddUpdateGlobalJsonService(this IServiceCollection services)
        {
            services.AddConsoleService();

            // services.AddUpdateLocalSolutionFile();
            services.AddCloneReposAndUpdateAll();

            services.AddSingletonIfNotExists<UpdateGlobalJsonService>();
        }
    }

    internal sealed class UpdateGlobalJsonService(IEnumerable<IUpdateGlobalJsonServiceStrategy> fixServiceRegistrationsStrategies)
    {
        public Task HandleAsync(UpdateGlobalJsonParameters parameters)
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

    internal interface IUpdateGlobalJsonServiceStrategy
    {
        bool CanHandle(UpdateGlobalJsonParameters parameters);

        Task HandleAsync(UpdateGlobalJsonParameters parameters);
    }
}
