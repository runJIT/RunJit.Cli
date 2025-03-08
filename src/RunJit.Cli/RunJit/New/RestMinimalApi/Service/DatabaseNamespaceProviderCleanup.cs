using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;
using RunJit.Cli.Services;

namespace RunJit.Cli.New.RestMinimalApi
{
    internal static class AddDatabaseNamespaceProviderCleanupExtension
    {
        internal static void AddDatabaseNamespaceProviderCleanup(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRestMinimalApiSpecificCodeGen, DatabaseNamespaceProviderCleanup>();
        }
    }

    internal sealed class DatabaseNamespaceProviderCleanup(ConsoleService consoleService,
                                                   NamespaceProvider namespaceProvider) : IRestMinimalApiSpecificCodeGen, IRestMinimalApiTestSpecificCodeGen
    {
        public Task GenerateAsync(FileInfo solutionFileInfo,
                                  FileInfo webApiProject,
                                  CreateRestApiInfos createRestApiInfos)
        {
            // We have to setup all namespace providers correct
            // Database             
            // -> Projects         NamespaceProvider true
            //    -> Entities      NamespaceProvider false
            //    -> Migrations    NamespaceProvider false
            var databasePath = Path.Combine(webApiProject.Directory!.FullName, "Database");
            var databaseDirectory = new DirectoryInfo(databasePath);

            if (databaseDirectory.NotExists())
            {
                return Task.CompletedTask;
            }

            foreach (var directoryInfo in databaseDirectory.EnumerateDirectories("*", SearchOption.AllDirectories))
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

                // Database.Domain.XXX
                // Since level 3 everything have to be set to NamespaceProvider false

                var split = relativePath.Split(Path.DirectorySeparatorChar);
                if (split.Length < 3)
                {
                    continue;
                }

                namespaceProvider.SetNamespaceProvider(webApiProject, @namespace, false);
            }

            consoleService.WriteSuccess("Namespace provider successfully setup");

            return Task.CompletedTask;
        }
    }
}
