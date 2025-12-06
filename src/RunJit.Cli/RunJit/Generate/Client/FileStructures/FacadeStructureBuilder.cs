using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Generate.Client
{
    internal static class AddFacadeStructureBuilderExtension
    {
        internal static void AddFacadeStructureBuilder(this IServiceCollection services)
        {
            services.AddDomainFolderBuilder();
            services.AddEndpointStructureBuilder();

            services.AddSingletonIfNotExists<FacadeStructureBuilder>();
        }
    }

    internal sealed class FacadeStructureBuilder(DomainFolderBuilder domainFolderBuilder,
                                                 EndpointStructureBuilder endpointStructureBuilder)
    {
        internal async Task CreateAsync(DirectoryInfo apiFolder,
                                        ImmutableList<GeneratedFacade> clientFacades,
                                        string projectName,
                                        string clientName)
        {
            foreach (var generatedFacade in clientFacades)
            {
                // 0. Create domain folder. Like Users, Resources
                var domainFolder = domainFolderBuilder.Build(apiFolder, generatedFacade);

                // 1. Write facade class like UsersFacade.cs, ResourcesFacade.cs
                var facadeFileInfo = new FileInfo(Path.Combine(domainFolder.FullName, $"{generatedFacade.FacadeName}.cs"));
                await File.WriteAllTextAsync(facadeFileInfo.FullName, generatedFacade.SyntaxTree).ConfigureAwait(false);

                // 2. Write endpoint file structures
                await endpointStructureBuilder.CreateAsync(domainFolder, generatedFacade.Endpoints, projectName,
                                                           clientName, facadeFileInfo).ConfigureAwait(false);
            }
        }
    }
}
