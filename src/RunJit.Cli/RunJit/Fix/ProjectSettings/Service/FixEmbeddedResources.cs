using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;

namespace RunJit.Cli.Fix.ProjectSettings
{
    internal static class AddFixProjectSettingsExtension
    {
        internal static void AddFixProjectSettings(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddFixProjectSettingsParameters();
            services.AddCloneReposAndUpdateAll();

            services.AddSingletonIfNotExists<IFixProjectSettings, FixProjectSettings>();
        }
    }

    internal interface IFixProjectSettings
    {
        Task HandleAsync(FixProjectSettingsParameters parameters);
    }

    internal sealed class FixProjectSettings(IEnumerable<IFixProjectSettingsStrategy> fixServiceRegistrationsStrategies) : IFixProjectSettings
    {
        public Task HandleAsync(FixProjectSettingsParameters parameters)
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

    internal interface IFixProjectSettingsStrategy
    {
        bool CanHandle(FixProjectSettingsParameters parameters);

        Task HandleAsync(FixProjectSettingsParameters parameters);
    }
}
