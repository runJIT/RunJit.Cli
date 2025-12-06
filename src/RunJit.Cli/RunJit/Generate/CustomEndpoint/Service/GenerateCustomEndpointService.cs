using System.Collections.Immutable;
using Argument.Check;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.RunJit.Generate.CustomEndpoint
{
    internal static class AddGenerateCustomEndointServiceExtension
    {
        internal static void AddGenerateCustomEndointService(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<GenerateCustomEndpointService>();
        }
    }

    internal sealed class GenerateCustomEndpointService(ConsoleService consoleService)
    {
        public async Task GenerateAsync(GenerateCustomEndpointParameters parameters)
        {
            if (parameters.TargetFolder.NotExists())
            {
                parameters.TargetFolder.Create();
            }

            var endpointData = parameters.EndpointData;

            if (endpointData.EndsWith(".json"))
            {
                endpointData = await File.ReadAllTextAsync(endpointData).ConfigureAwait(false);
            }

            var deserializedData = endpointData.FromJsonStringAs<EndpointData>();

            // I need a recursive function with a trampolin pattern to iterate over the endpointData.Templates and create the folders and files
            await CreateFoldersAndFiles(parameters.TargetFolder, deserializedData.Templates).ConfigureAwait(false);

            consoleService.WriteSuccess($"Endpoints successfully created for your endpoint data:{Environment.NewLine}{parameters.EndpointData}");
        }

        private async Task CreateFoldersAndFiles(DirectoryInfo directoryInfo,
                                                 ImmutableList<Template> endpointDataTemplates)
        {
            foreach (var template in endpointDataTemplates)
            {
                var folder = new DirectoryInfo(Path.Combine(directoryInfo.FullName, template.Folder));

                if (folder.NotExists())
                {
                    folder.Create();
                }

                foreach (var file in template.Files)
                {
                    // ToDo: Replacement, AI magic here.
                    //       Startup.cs
                    var fileInfo = new FileInfo(Path.Combine(folder.FullName, file.Name));
                    var fileContent = file.Content;

                    if (Path.IsPathFullyQualified(file.Content))
                    {
                        var fileContentAsFileInfo = new FileInfo(file.Content);
                        Throw.IfNotExists(fileContentAsFileInfo);

                        fileContent = await File.ReadAllTextAsync(fileContentAsFileInfo.FullName).ConfigureAwait(false);
                    }

                    await File.WriteAllTextAsync(fileInfo.FullName, fileContent).ConfigureAwait(false);
                }

                await CreateFoldersAndFiles(folder, template.Templates).ConfigureAwait(false);
            }
        }
    }
}
