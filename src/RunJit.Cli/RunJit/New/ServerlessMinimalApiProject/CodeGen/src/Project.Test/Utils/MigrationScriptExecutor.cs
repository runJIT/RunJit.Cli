using System.Reflection;
using DotNetTool.Service;
using Extensions.Pack;
using StringExtensions = Extensions.Pack.StringExtensions;

namespace $ProjectName$.Test.Utils
{
    internal sealed class MigrationScriptExecutor(IDotNetTool dotNetTool)
    {
        internal async Task RunMigrationScriptsAsync(Assembly assembly)
        {
            // Important: Cause of startup performance and current infrastructure design
            //            the migration scripts are not executed as expected in the API startup!
            //            All databases are already pre-setup (with all pros and cons)
            var migrationScriptFiles = assembly.GetManifestResourceNames()
                                               .Where(name => name.Contains("Database.Migrations."))
                                               .ToList();

            foreach (var migrationScriptAsFile in migrationScriptFiles)
            {
                var migrationScript = assembly.GetFileContentFrom(migrationScriptAsFile);
                var normalizeScript = migrationScript.Replace("aws ", string.Empty);

                var oneLine = normalizeScript.Replace(Environment.NewLine, string.Empty)
                                             .Replace("\n", string.Empty)
                                             .Replace(@" \", string.Empty);
                
                var migrationScriptRunResult = await dotNetTool.RunAsync("aws", oneLine).ConfigureAwait(false);
                Assert.AreEqual(0, migrationScriptRunResult.ExitCode, $"Dynamo DB: MigrationsScript {migrationScriptAsFile} could not be executed. {migrationScriptRunResult.Output}");
            }
        }
    }
}
