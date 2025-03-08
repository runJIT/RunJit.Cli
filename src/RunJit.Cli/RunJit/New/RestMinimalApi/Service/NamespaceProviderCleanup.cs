using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;
using RunJit.Cli.Services;

namespace RunJit.Cli.New.RestMinimalApi
{
    internal static class AddNamespaceProviderCleanupExtension
    {
        internal static void AddNamespaceProviderCleanup(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRestMinimalApiSpecificCodeGen, NamespaceProviderCleanup>();
            services.AddSingletonIfNotExists<IRestMinimalApiTestSpecificCodeGen, NamespaceProviderCleanup>();
        }
    }

    internal sealed class NamespaceProviderCleanup(ConsoleService consoleService,
                                                   NamespaceProvider namespaceProvider) : IRestMinimalApiSpecificCodeGen, IRestMinimalApiTestSpecificCodeGen
    {
        public Task GenerateAsync(FileInfo solutionFileInfo,
                                  FileInfo webApiProject,
                                  CreateRestApiInfos createRestApiInfos)
        {
            // We have to setup all namespace providers correct
            // API 
            // -> V1 (Version)  NamespaceProvider true
            //    -> Create     NamespaceProvider false
            //    -> Update     NamespaceProvider false
            // -> V2 (Version)  NamespaceProvider true
            //    -> Create     NamespaceProvider false
            //    -> Update     NamespaceProvider false
            var apiFolderPath = Path.Combine(webApiProject.Directory!.FullName, "Api");
            var apiDirectoryInfo = new DirectoryInfo(apiFolderPath);

            if (apiDirectoryInfo.NotExists())
            {
                return Task.CompletedTask;
            }

            foreach (var directoryInfo in apiDirectoryInfo.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(webApiProject.Directory!.FullName, directoryInfo.FullName);
                var @namespace = relativePath.Replace(Path.DirectorySeparatorChar, '.');

                if (relativePath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                {
                    continue;
                }
                
                if (relativePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                {
                    continue;
                }

                // If directory name == V1 or V2 then NamespaceProvider must be true
                if (GlobalRegex.VersionRegex().IsMatch(directoryInfo.Name))
                {
                    namespaceProvider.SetNamespaceProvider(webApiProject, @namespace, true);
                }

                // Path must contain the version part
                // -> Api\Projects\V1\Create
                // To skip if: 
                // -> Api\Projects\V1  -> ending version
                if (GlobalRegex.VersionRegex().IsMatch(relativePath) &&
                    GlobalRegex.VersionRegex().IsMatch(relativePath.Split(Path.DirectorySeparatorChar).LastOrDefault() ?? string.Empty).IsFalse())
                {
                    namespaceProvider.SetNamespaceProvider(webApiProject, @namespace, false);
                }
            }

            consoleService.WriteSuccess("Namespace provider successfully setup");

            return Task.CompletedTask;
        }
    }
}
