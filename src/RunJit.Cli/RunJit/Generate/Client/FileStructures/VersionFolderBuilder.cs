using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Generate.Client
{
    internal static class AddVersionFolderBuilderExtension
    {
        internal static void AddVersionFolderBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<VersionFolderBuilder>();
        }
    }

    internal sealed class VersionFolderBuilder
    {
        internal DirectoryInfo Build(DirectoryInfo apiFolder,
                                     GeneratedClientCodeForController generatedFacade)
        {
            var versionFolder = generatedFacade.ControllerInfo.Version.IsNotNull() ? new DirectoryInfo(Path.Combine(apiFolder.FullName, generatedFacade.ControllerInfo.Version.Normalized)) : apiFolder;

            if (versionFolder.Exists.IsFalse())
            {
                versionFolder.Create();
            }

            return versionFolder;
        }
    }
}
