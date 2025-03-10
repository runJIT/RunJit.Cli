using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Services.Resharper
{
    internal static class AddSolutionCodeCleanupExtension
    {
        internal static void AddSolutionCodeCleanup(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<SolutionCodeCleanup>();
        }
    }

    internal sealed class SolutionCodeCleanup(DotNetTool.Service.DotNetTool dotnetTool,
                                              ConsoleService consoleService)
    {
        private const string ResharperToolName = "JetBrains.ReSharper.GlobalTools";

        internal async Task CleanupSolutionAsync(FileInfo solutionFile)
        {
            // 1. Check that R# dotnet tool is installed
            consoleService.WriteInfo($"Check if {ResharperToolName} are already installed");
            var resharperTool = await dotnetTool.ExistsAsync(ResharperToolName).ConfigureAwait(false);

            if (resharperTool.IsNull())
            {
                consoleService.WriteInfo($"Try to install {ResharperToolName} for code inspection and cleanups");
                var installResult = await dotnetTool.InstallAsync(ResharperToolName).ConfigureAwait(false);

                if (installResult.ExitCode == 0)
                {
                    consoleService.WriteSuccess($"Installation of the {ResharperToolName} was successful.{Environment.NewLine}{installResult.Output}");
                }
                else
                {
                    consoleService.WriteError($"Installation of the {ResharperToolName} failed.{Environment.NewLine}{installResult.Output}");
                }
            }
            else
            {
                consoleService.WriteInfo($"Try to update {ResharperToolName} for code inspection and cleanups");
                var updateResult = await dotnetTool.UpdateAsync(ResharperToolName).ConfigureAwait(false);

                if (updateResult.ExitCode == 0)
                {
                    consoleService.WriteSuccess($"Installation of the {ResharperToolName} was successful.{Environment.NewLine}{updateResult.Output}");
                }
                else
                {
                    consoleService.WriteError($"Installation of the {ResharperToolName} failed.{Environment.NewLine}{updateResult.Output}");
                }
            }

            // 2. Check for R# dot settings if existed, if not create one
            var dotSettingsFile = new FileInfo($"{solutionFile.FullName}.DotSettings");

            if (dotSettingsFile.NotExists())
            {
                var fileContent = EmbeddedFile.GetFileContentFrom("Resharper.sln.DotSettings");
                await File.WriteAllTextAsync(dotSettingsFile.FullName, fileContent).ConfigureAwait(false);
            }

            // 3. Update .editorconfig must be in sync with the .DotSettings file
            var editorConfigFile = new FileInfo(Path.Combine($"{solutionFile.Directory!.FullName}", ".editorconfig"));
            var editorConfig = EmbeddedFile.GetFileContentFrom(".editorconfig");
            await File.WriteAllTextAsync(editorConfigFile.FullName, editorConfig).ConfigureAwait(false);

            // 4. Get the absolute installation path of the .dotnet/tools directory
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var dotnetToolDirectory = new DirectoryInfo(Path.Combine(userProfile, ".dotnet", "tools"));

            // 5. Find absolute path of the installed R# dotnet tool
            var jbTool = dotnetToolDirectory.EnumerateFiles("jb*").FirstOrDefault();

            if (jbTool.IsNull())
            {
                consoleService.WriteError($"JetBrains.ReSharper.GlobalTools is not installed or not found in the .dotnet/tools directory '{dotnetToolDirectory.FullName}'");

                return;
            }

            // 6. Run R# code cleanup
            consoleService.WriteInfo($"Start C# code cleanup for solution: {solutionFile.FullName}");
            var cleanupResult = await dotnetTool.RunAsync(jbTool.FullName, $"cleanupcode {solutionFile.FullName} --settings={dotSettingsFile.FullName}").ConfigureAwait(false);

            // 7. Print execution result
            if (cleanupResult.ExitCode == 0)
            {
                consoleService.WriteSuccess($"C# Code cleanup in solution {solutionFile.FullName} was successful");
            }
            else
            {
                consoleService.WriteError($"C# Code cleanup in solution {solutionFile.FullName} failed");
            }
        }
    }
}
