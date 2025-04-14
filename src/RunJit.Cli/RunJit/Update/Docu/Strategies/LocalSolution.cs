using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;

namespace RunJit.Cli.Update.Docu
{
    internal static class AddLocalSolutionExtension
    {
        internal static void AddLocalSolution(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConsoleService();
            services.AddGitService();
            services.AddDotNet();
            services.AddAwsCodeCommit();
            services.AddFindSolutionFile();
            // services.AddAiDocuService(configuration);

            services.AddSingletonIfNotExists<IUpdateDocuServiceStrategy, LocalSolution>();
        }
    }

    internal sealed class LocalSolution() : IUpdateDocuServiceStrategy
    {
        public bool CanHandle(UpdateDocuParameters parameters)
        {
            return parameters.SolutionFile.IsNotNullOrWhiteSpace();
        }

        public Task HandleAsync(UpdateDocuParameters parameters)
        {
            throw new NotImplementedException();

            //// 0. Check that precondition is met
            //// Ensure that the parameters meet the required conditions before proceeding.
            //if (CanHandle(parameters).IsFalse())
            //{
            //    throw new RunJitException($"Please call {nameof(IUpdateGlobalJsonServiceStrategy.CanHandle)} before calling {nameof(IUpdateGlobalJsonServiceStrategy.HandleAsync)}");
            //}


            //var parsedSolution = new SolutionFileInfo(parameters.SolutionFile).Parse();

            //var cSharpSyntaxTrees = parsedSolution.ProductiveProjects.SelectMany(p => p.CSharpFileInfos)
            //                                      .Select(c => c.Parse())
            //                                      .ToImmutableList();

            //foreach (var cSharpSyntaxTree in cSharpSyntaxTrees)
            //{
            //    foreach (var @class in cSharpSyntaxTree.Classes)
            //    {
            //        // Detect command,endpoint and so on
            //        if (@class.Name.EndsWith("Endpoint"))
            //        {
            //            var documentation = await aiDocuService.DocuEndpointAsync(@class.SyntaxTree).ConfigureAwait(false);
            //            Console.WriteLine(documentation);
            //        }
            //    }
            //}



            // Log success message for the processed solution.
            // consoleService.WriteSuccess($"Solution: {parameters.SolutionFile} was successfully checked and was buildable");
        }
    }
}
