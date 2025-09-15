using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Commands;
using RunJit.Cli.Test.Extensions;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("runjit update resharper settings")]
    [TestClass]
    public class UpdateResharperSettings : GlobalSetup
    {
        [TestMethod]
        public async Task Should_Update_Resharper_Settings_Tests()
        {
            // 1. Create new Web Api
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi("RunJit.Update.Resharper.Settings", WebApiFolder, "api/resharper")).ConfigureAwait(false);

            // 3. Test if generated results is buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);

            // 3. Update to .Net 8
            await Mediator.SendAsync(new UpdateResharperSettingsForSolution(solutionFile)).ConfigureAwait(false);
        }

        [Ignore("Dev purpose only")]
        [TestMethod]
        [DataRow(@"D:\ResharperSettingsUpdate\pulse-core-service\pulse.core.service.sln")]
        public async Task Should_Update_A_Specific_Solution_With_New_Resharper_Settings_Tests(string solution)
        {
            // 1. Create new Web Api
            var solutionFile = new FileInfo(solution);

            // 3. Test if generated results is buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);

            // 3. Update to .Net 8
            await Mediator.SendAsync(new UpdateResharperSettingsForSolution(solutionFile)).ConfigureAwait(false);
        }
    }

    internal sealed record UpdateResharperSettingsForSolution(FileInfo solution) : ICommand;

    internal sealed class UpdateResharperSettingsHandler : ICommandHandler<UpdateResharperSettingsForSolution>
    {
        public async Task Handle(UpdateResharperSettingsForSolution request,
                                 CancellationToken cancellationToken)
        {
            await using var sw = new StringWriter();
            Console.SetOut(sw);

            var strings = CollectConsoleParameters(request).ToArray();
            var consoleCall = strings.Flatten(" ");
            Console.WriteLine();
            Console.WriteLine(consoleCall);
            Debug.WriteLine(consoleCall);
            var exitCode = await Program.Main(strings).ConfigureAwait(false);
            var output = sw.ToString();

            Assert.AreEqual(0, exitCode, output);
        }

        private IEnumerable<string> CollectConsoleParameters(UpdateResharperSettingsForSolution parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";
            yield return "resharpersettings";
            yield return "--solution";
            yield return parameters.solution.FullName;
        }
    }

    internal sealed record UpdateResharperSettingsForGitRepos(string GitRepos,
                                                              string WorkingDirectory) : ICommand;

    internal sealed class UpdateResharperSettingsForGitReposHandler : ICommandHandler<UpdateResharperSettingsForGitRepos>
    {
        public async Task Handle(UpdateResharperSettingsForGitRepos request,
                                 CancellationToken cancellationToken)
        {
            await using var sw = new StringWriter();
            Console.SetOut(sw);

            var strings = CollectConsoleParameters(request).ToArray();
            var consoleCall = strings.Flatten(" ");
            Console.WriteLine();
            Console.WriteLine(consoleCall);
            Debug.WriteLine(consoleCall);
            var exitCode = await Program.Main(strings).ConfigureAwait(false);
            var output = sw.ToString();

            Assert.AreEqual(0, exitCode, output);
        }

        private IEnumerable<string> CollectConsoleParameters(UpdateResharperSettingsForGitRepos parameters)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "update";

            yield return "resharpersettings";
            yield return "--git-repos";
            yield return parameters.GitRepos;
            yield return "--working-directory";
            yield return parameters.WorkingDirectory;
        }
    }
}
