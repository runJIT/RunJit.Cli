using System.Runtime.InteropServices;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services.Net;

namespace RunJit.Cli.Services
{
    internal static class AddRiderExtension
    {
        internal static void AddRider(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IIDE, Rider>();
        }
    }

    // JetBrains Rider implementation with lazy caching.
    internal class Rider : IIDE
    {
        private readonly IDotNet _dotNet;

        public string Name => "JetBrains Rider";

        public int Priority => 2;

        private readonly Lazy<FileInfo?> _cachedRiderFile;

        public Rider(IDotNet dotNet)
        {
            _dotNet = dotNet;
            _cachedRiderFile = new Lazy<FileInfo?>(FindIde);
        }

        private FileInfo? FindIde()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string jetBrainsPath = Path.Combine(programFiles, "JetBrains");

                if (!Directory.Exists(jetBrainsPath))
                {
                    return null;
                }

                // Look for folders starting with "JetBrains Rider"
                var riderDirs = Directory.GetDirectories(jetBrainsPath, "JetBrains Rider*");

                // Attempt to parse the version from the folder name after "JetBrains Rider".
                var parsed = new List<(Version version, string path)>();

                foreach (var dir in riderDirs)
                {
                    string folderName = Path.GetFileName(dir);

                    // Remove "JetBrains Rider" and trim to get the version part.
                    string suffix = folderName.Replace("JetBrains Rider", "").Trim();

                    if (!suffix.IsNullOrEmpty())
                    {
                        if (Version.TryParse(suffix, out var version))
                        {
                            parsed.Add((version, dir));
                        }
                    }
                }

                if (parsed.Any())
                {
                    // Get the newest version.
                    var newest = parsed.OrderByDescending(x => x.version).First();
                    string riderExe = Path.Combine(newest.path, "bin", "rider64.exe");
                    var fi = new FileInfo(riderExe);

                    if (fi.Exists)
                    {
                        return fi;
                    }
                }

                // Fallback check if a versioned folder wasn't found.
                string fallbackExe = Path.Combine(jetBrainsPath, "Rider", "bin",
                                                  "rider64.exe");

                var fallbackFile = new FileInfo(fallbackExe);

                return fallbackFile.Exists ? fallbackFile : null;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string macPath = "/Applications/Rider.app";

                return Directory.Exists(macPath) ? new FileInfo(macPath) : null;
            }

            return null;
        }

        public bool IsInstalled() => _cachedRiderFile.Value != null;

        public Task LaunchAsync(FileInfo solutionFileInfo)
        {
            var vsFile = _cachedRiderFile.Value;

            if (vsFile.IsNull())
            {
                return Task.CompletedTask;
            }

            return _dotNet.RunAsync(vsFile.FullName, solutionFileInfo.FullName);
        }
    }
}
