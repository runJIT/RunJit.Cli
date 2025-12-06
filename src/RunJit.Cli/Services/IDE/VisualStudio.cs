using System.Runtime.InteropServices;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services.Net;

namespace RunJit.Cli.Services
{
    namespace IDELauncherExample
    {
        public static class AddVisualStudioExtension
        {
            public static void AddVisualStudio(this IServiceCollection services)
            {
                services.AddSingletonIfNotExists<IIDE, VisualStudio>();
            }
        }

        // Visual Studio implementation with lazy caching of the discovered executable.
        internal class VisualStudio : IIDE
        {
            private readonly IDotNet _dotNet;

            public string Name => "Visual Studio";

            public int Priority => 1;

            // Look for preferred editions.
            readonly string[] _preferredEditions =
            {
                "Enterprise", "Professional", "Community",
                "Preview"
            };

            // Cache the discovered executable (or bundle path on macOS).
            private readonly Lazy<FileInfo?> _cachedVsFile;

            public VisualStudio(IDotNet dotNet)
            {
                _dotNet = dotNet;
                _cachedVsFile = new Lazy<FileInfo?>(FindIde);
            }

            private FileInfo? FindIde()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    string vsBasePath = Path.Combine(programFilesX86, "Microsoft Visual Studio");

                    if (!Directory.Exists(vsBasePath))
                    {
                        return null;
                    }

                    // Look for subdirectories whose names can be parsed as years (e.g. "2019", "2022").
                    var yearDirectories = Directory.GetDirectories(vsBasePath)
                                                   .Where(dir => int.TryParse(Path.GetFileName(dir), out _))
                                                   .ToList();

                    // Get the newest version (highest year).
                    var newestYearDirectory = yearDirectories.OrderByDescending(dir => int.Parse(Path.GetFileName(dir)))
                                                             .FirstOrDefault();

                    if (newestYearDirectory.IsNull())
                    {
                        return null;
                    }

                    string? editionPath = null;

                    foreach (var edition in _preferredEditions)
                    {
                        string potentialPath = Path.Combine(newestYearDirectory, edition);

                        if (!Directory.Exists(potentialPath))
                        {
                            continue;
                        }

                        editionPath = potentialPath;

                        break;
                    }

                    if (editionPath.IsNull())
                    {
                        return null;
                    }

                    string finalPath = Path.Combine(editionPath, "Common7", "IDE",
                                                    "devenv.exe");

                    var fileInfo = new FileInfo(finalPath);

                    return fileInfo.Exists ? fileInfo : null;
                }

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return null;
                }

                // On macOS, Visual Studio is an app bundle.
                string vsApp = "/Applications/Visual Studio.app";

                return Directory.Exists(vsApp) ? new FileInfo(vsApp) : null;
            }

            public bool IsInstalled() => _cachedVsFile.Value != null;

            public Task LaunchAsync(FileInfo solutionFileInfo)
            {
                var vsFile = _cachedVsFile.Value;

                if (vsFile.IsNull())
                {
                    return Task.CompletedTask;
                }

                return _dotNet.RunAsync(vsFile.FullName, solutionFileInfo.FullName);
            }
        }
    }
}
