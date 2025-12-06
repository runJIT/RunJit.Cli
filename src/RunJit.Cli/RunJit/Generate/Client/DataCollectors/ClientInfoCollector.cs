using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Models;

namespace RunJit.Cli.RunJit.Generate.Client
{
    internal static class AddClientInfoCollectorExtension
    {
        internal static void AddClientInfoCollector(this IServiceCollection services)
        {
            services.AddCollectSolutionPath();
            services.AddCollectTargetPath();
            services.AddCollectSwaggerPath();

            services.AddSingletonIfNotExists<IClientInfoCollector, ClientInfoCollector>();
        }
    }

    internal interface IClientInfoCollector
    {
        Client Collect(ClientParameters clientGenParameters);
    }

    // ToDo: ClientGen Info collector
    internal sealed class ClientInfoCollector(CollectSolutionPath collectSolutionPath,
                                              CollectTargetPath collectTargetPath) : IClientInfoCollector
    {
        private readonly CollectTargetPath _collectTargetPath = collectTargetPath;

        public Client Collect(ClientParameters clientGenParameters)
        {
            var solutionFileExists = clientGenParameters.SolutionFile.IsNotNull() && clientGenParameters.SolutionFile.Exists;

            // 1. Request backend solution
            var solutionFileInfo = solutionFileExists ? clientGenParameters.SolutionFile : collectSolutionPath.Collect();

            // If integrate into source solution we enter the solution file :)
            var targetDirectory = solutionFileInfo;

            var projectName = $"{solutionFileInfo.NameWithoutExtension()}.Client";
            var dotnetToolName = new DotNetToolName("dotnet-clientgen", "clientgen");

            return new Client(projectName, dotnetToolName, solutionFileInfo, targetDirectory, clientGenParameters.UseOpenApiJson);
        }
    }
}
