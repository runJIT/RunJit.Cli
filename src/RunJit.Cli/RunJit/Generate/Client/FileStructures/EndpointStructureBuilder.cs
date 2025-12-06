using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Generate.Client
{
    internal static class AddEndpointStructureBuilderExtension
    {
        internal static void AddEndpointStructureBuilder(this IServiceCollection services)
        {
            services.AddVersionFolderBuilder();
            services.AddModelFolderBuilder();
            services.AddModelsToFilesWriter();

            services.AddSingletonIfNotExists<EndpointStructureBuilder>();
        }
    }

    internal sealed class EndpointStructureBuilder(VersionFolderBuilder versionFolderBuilder,
                                                   ModelFolderBuilder modelFolderBuilder,
                                                   ModelsToFilesWriter modelsToFileWriter)
    {
        internal async Task CreateAsync(DirectoryInfo domainFolder,
                                        ImmutableList<GeneratedClientCodeForController> endpoints,
                                        string projectName,
                                        string clientName,
                                        FileInfo facadeFileInfo)
        {
            foreach (var endpoint in endpoints)
            {
                // NEW quickfix
                // No version means no facade -> endpoint syntaxtree become the facade
                // In future we have to refactor the whole flow
                if (endpoint.ControllerInfo.Version.IsNull())
                {
                    // Dirty hack :/
                    var syntaxTree = endpoint.SyntaxTree.Replace($" {domainFolder.Name}", $" {domainFolder.Name}Facade")
                                             .Replace($"Add{domainFolder.Name}", $"Add{domainFolder.Name}Facade");

                    await File.WriteAllTextAsync(facadeFileInfo.FullName, syntaxTree).ConfigureAwait(false);

                    var modelFolder = modelFolderBuilder.Build(facadeFileInfo.Directory!);

                    var modelsToWrite = endpoint.ControllerInfo.Endpoints.SelectMany(m => m.Models).ToImmutableList();

                    // 4. Write all models to files
                    await modelsToFileWriter.WriteAsync(modelFolder, endpoint, modelsToWrite,
                                                        projectName, clientName).ConfigureAwait(false);

                    continue;
                }

                // 0. Create version folder like V1, V2, V3
                var versionFolder = versionFolderBuilder.Build(domainFolder, endpoint);

                // 1. Write domain version class like ResourceV1.cs
                var pathVersionClient = Path.Combine(versionFolder.FullName, $"{endpoint.Domain}.cs");
                await File.WriteAllTextAsync(pathVersionClient, endpoint.SyntaxTree).ConfigureAwait(false);

                // 2. Create models folders like Models
                var modelsFolder = modelFolderBuilder.Build(versionFolder);

                // 3. Get all models / data types
                var dataTypes = endpoint.ControllerInfo.Endpoints.SelectMany(m => m.Models).ToImmutableList();

                // 4. Write all models to files
                await modelsToFileWriter.WriteAsync(modelsFolder, endpoint, dataTypes,
                                                    projectName, clientName).ConfigureAwait(false);
            }
        }
    }
}
