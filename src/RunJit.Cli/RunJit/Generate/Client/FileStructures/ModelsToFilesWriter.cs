using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.Client;
using Solution.Parser.CSharp;

namespace RunJit.Cli.RunJit.Generate.Client
{
    internal static class AddModelsToFilesWriterExtension
    {
        internal static void AddModelsToFilesWriter(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ModelsToFilesWriter>();
        }
    }

    internal sealed class ModelsToFilesWriter(ModelBuilder modelBuilder)
    {
        public async Task WriteAsync(DirectoryInfo modelsFolder,
                                     GeneratedClientCodeForController controller,
                                     ImmutableList<DeclarationBase> dataTypes,
                                     string projectName,
                                     string clientName)
        {
            foreach (var dataType in dataTypes)
            {
                var model = modelBuilder.BuildFrom(dataType, controller, projectName,
                                                   clientName);

                await File.WriteAllTextAsync(Path.Combine(modelsFolder.FullName, $"{dataType.Name}.cs"), model).ConfigureAwait(false);
            }
        }
    }
}
