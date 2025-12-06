using System.Runtime.InteropServices;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services.Net;

namespace RunJit.Cli.Services
{
    internal static class AddVisualStudioCodeExtension
    {
        internal static void AddVisualStudioCode(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IIDE, VisualStudioCode>();
        }
    }

    // Visual Studio Code implementation with lazy caching.
    internal class VisualStudioCode : IIDE
    {
        private readonly IDotNet _dotNet;

        public string Name => "Visual Studio Code";

        public int Priority => 3;

        private readonly Lazy<FileInfo?> _cachedCodeFile;

        public VisualStudioCode(IDotNet dotNet)
        {
            _dotNet = dotNet;
            _cachedCodeFile = new Lazy<FileInfo?>(FindIde);
        }

        private FileInfo? FindIde()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string codePath = Path.Combine(programFiles, "Microsoft VS Code", "Code.exe");
                var fileInfo = new FileInfo(codePath);

                if (fileInfo.Exists)
                {
                    return fileInfo;
                }

                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                string codeUserPath = Path.Combine(localAppData, "Programs", "Microsoft VS Code",
                                                   "Code.exe");

                var fileInfo2 = new FileInfo(codeUserPath);

                return fileInfo2.Exists ? fileInfo2 : null;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string macPath = "/Applications/Visual Studio Code.app";

                return Directory.Exists(macPath) ? new FileInfo(macPath) : null;
            }

            return null;
        }

        public bool IsInstalled() => _cachedCodeFile.Value != null;

        public Task LaunchAsync(FileInfo solutionFileInfo)
        {
            var vsFile = _cachedCodeFile.Value;

            if (vsFile.IsNull())
            {
                return Task.CompletedTask;
            }

            return _dotNet.RunAsync(vsFile.FullName, solutionFileInfo.FullName);
        }
    }
}
