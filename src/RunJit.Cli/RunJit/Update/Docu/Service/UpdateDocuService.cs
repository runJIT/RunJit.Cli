using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;

namespace RunJit.Cli.Update.Docu
{
    internal static class AddUpdateDocuServiceExtension
    {
        internal static void AddUpdateDocuService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConsoleService();

            // services.AddUpdateLocalSolutionFile();
            services.AddCloneReposAndUpdateAll();
            services.AddLocalSolution(configuration);

            services.AddSingletonIfNotExists<UpdateDocuService>();
        }
    }

    internal sealed class UpdateDocuService(IEnumerable<IUpdateDocuServiceStrategy> fixServiceRegistrationsStrategies)
    {
        public Task HandleAsync(UpdateDocuParameters parameters)
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

    internal interface IUpdateDocuServiceStrategy
    {
        bool CanHandle(UpdateDocuParameters parameters);

        Task HandleAsync(UpdateDocuParameters parameters);
    }
}
