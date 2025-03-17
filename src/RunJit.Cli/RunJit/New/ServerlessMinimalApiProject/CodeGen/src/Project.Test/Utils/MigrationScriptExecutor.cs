using System.Reflection;
using DotNetTool.Service;
using Extensions.Pack;
using Polly;

namespace $ProjectName$.Test.Utils
{
    internal sealed class MigrationScriptExecutor(IDotNetTool dotNetTool)
    {
        private readonly AsyncPolicy _asyncRetryPolicy = Policy.Handle<Exception>()
                                                   .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        internal async Task RunMigrationScriptsAsync(Assembly assembly)
        {
            // Important: Cause of startup performance and current infrastructure design
            //            the migration scripts are not executed as expected in the API startup!
            //            All databases are already pre-setup (with all pros and cons)
            var migrationScriptFiles = assembly.GetManifestResourceNames()
                                               .Where(name => name.Contains("Database") &&
                                                              name.EndsWith(".sh"))
                                               .ToList();

            foreach (var migrationScriptAsFile in migrationScriptFiles)
            {
                var migrationScript = assembly.GetFileContentFrom(migrationScriptAsFile);
                var normalizeScript = migrationScript.Replace("aws ", string.Empty);

                var command = normalizeScript.Replace(Environment.NewLine, string.Empty)
                                             .Replace("\n", string.Empty)
                                             .Replace(@" \", string.Empty);


                // Important use policy cause of start/stop of docker container can cause delays from time to time
                await _asyncRetryPolicy.ExecuteAsync(() => ExecuteDbMigrationScriptsAsync(command, migrationScriptAsFile)).ConfigureAwait(false);
            }
        }

        private async Task ExecuteDbMigrationScriptsAsync(string command, string migrationScriptAsFile)
        {
            var migrationScriptRunResult = await dotNetTool.RunAsync("aws", command).ConfigureAwait(false);
            Assert.AreEqual(0, migrationScriptRunResult.ExitCode, $"Dynamo DB: MigrationsScript {migrationScriptAsFile} could not be executed. {migrationScriptRunResult.Output}");
        }
    }
}
