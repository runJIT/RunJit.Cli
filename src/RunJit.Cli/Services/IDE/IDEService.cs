using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services.IDELauncherExample;

namespace RunJit.Cli.Services
{
    internal static class AddIDEServiceExtension
    {
        internal static void AddIDEService(this IServiceCollection services)
        {
            services.AddRider();
            services.AddVisualStudio();
            services.AddVisualStudioCode();

            services.AddSingletonIfNotExists<IDEService>();
        }
    }

    internal interface IIDE
    {
        string Name { get; }

        int Priority { get; } // Lower number means higher priority.

        bool IsInstalled();

        Task LaunchAsync(FileInfo solutionFile);
    }

    internal class IDEService(IEnumerable<IIDE> ides)
    {
        public void LaunchIdeFireAndForget(FileInfo solutionPath)
        {
            Task.Run(() => LaunchIdeAsync(solutionPath));
        }

        public async Task LaunchIdeAsync(FileInfo solutionPath)
        {
            // Find the first installed IDE, ordered by the defined priority.
            var availableIde = ides.Where(ide => ide.IsInstalled())
                                   .OrderBy(ide => ide.Priority)
                                   .ToList();

            var ideToStart = availableIde.FirstOrDefault();

            if (ideToStart.IsNotNull())
            {
                Console.WriteLine($"{ideToStart.Name} is available. Launching it...");
                await ideToStart.LaunchAsync(solutionPath).ConfigureAwait(false);
            }
            else
            {
                Console.WriteLine("No supported IDE detected.");
            }
        }
    }
}
