using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;

namespace RunJit.Cli.Migrate.Endpoints.RegisterEndpoints.Service
{
    internal static class AddRegisterEndpointsExtension
    {
        internal static void AddRegisterEndpoints(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddRegisterEndpointsParameters();

            services.AddSingletonIfNotExists<IRegisterEndpointsStrategy, RegisterEndpointsStrategy>();
            services.AddSingletonIfNotExists<IRegisterEndpoints, RegisterEndpoints>();
        }
    }

    internal interface IRegisterEndpoints
    {
        Task HandleAsync(RegisterEndpointsParameters parameters);
    }

    internal sealed class RegisterEndpoints(IEnumerable<IRegisterEndpointsStrategy> strategies) : IRegisterEndpoints
    {
        public Task HandleAsync(RegisterEndpointsParameters parameters)
        {
            var strategy = strategies.FirstOrDefault(s => s.CanHandle(parameters));

            if (strategy is null)
            {
                throw new RunJitException($"No strategy found for parameters: {parameters}");
            }

            return strategy.HandleAsync(parameters);
        }
    }

    internal interface IRegisterEndpointsStrategy
    {
        bool CanHandle(RegisterEndpointsParameters parameters);

        Task HandleAsync(RegisterEndpointsParameters parameters);
    }

    internal sealed class RegisterEndpointsStrategy(ConsoleService consoleService) : IRegisterEndpointsStrategy
    {
        public bool CanHandle(RegisterEndpointsParameters parameters)
        {
            return !parameters.SolutionFile.IsNullOrWhiteSpace()
                   && !parameters.WebApiProject.IsNullOrWhiteSpace()
                   && !parameters.DomainNamePlural.IsNullOrWhiteSpace();
        }

        public Task HandleAsync(RegisterEndpointsParameters parameters)
        {
            // For now, only outputs guidance. Real implementation could parse and update Startup.cs like similar services do.
            consoleService.WriteInfo("Registering endpoints...\n" +
                                     $"Solution: {parameters.SolutionFile}\n" +
                                     $"WebApi:   {parameters.WebApiProject}\n" +
                                     $"Domain:   {parameters.DomainNamePlural}");

            return Task.CompletedTask;
        }
    }
}
