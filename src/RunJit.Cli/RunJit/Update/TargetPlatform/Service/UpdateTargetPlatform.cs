using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;

namespace RunJit.Cli.Update.TargetPlatform
{
    internal static class AddUpdateTargetPlatformExtension
    {
        internal static void AddUpdateTargetPlatform(this IServiceCollection services,
                                                     IConfiguration configuration)
        {
            services.AddConsoleService();
            services.AddUpdateTargetPlatformParameters();

            services.AddCloneReposAndUpdate(configuration);
            services.AddUpdateLocalSolutionFile();

            services.AddSingletonIfNotExists<IUpdateTargetPlatform, UpdateTargetPlatform>();
        }
    }

    internal interface IUpdateTargetPlatform
    {
        Task HandleAsync(UpdateTargetPlatformParameters parameters);
    }

    internal sealed class UpdateTargetPlatform(IEnumerable<IUpdateTargetPlatformStrategy> updateTargetPlatformStrategies) : IUpdateTargetPlatform
    {
        public Task HandleAsync(UpdateTargetPlatformParameters parameters)
        {
            var updateTargetPlatformStrategy = updateTargetPlatformStrategies.Where(x => x.CanHandle(parameters)).ToImmutableList();

            if (updateTargetPlatformStrategy.Count < 1)
            {
                throw new RunJitException($"Could not find a strategy a update nuget strategy for parameters: {parameters}");
            }

            if (updateTargetPlatformStrategy.Count > 1)
            {
                throw new RunJitException($"Found more than one strategy a update nuget strategy for parameters: {parameters}");
            }

            return updateTargetPlatformStrategy[0].HandleAsync(parameters);
        }
    }

    internal interface IUpdateTargetPlatformStrategy
    {
        bool CanHandle(UpdateTargetPlatformParameters parameters);

        Task HandleAsync(UpdateTargetPlatformParameters parameters);
    }
}
