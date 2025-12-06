using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;

namespace RunJit.Cli.RunJit.Update.ResharperSettings
{
    internal static class AddUpdateLocalSolutionFileExtension
    {
        internal static void AddUpdateLocalSolutionFile(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddGitService();
            services.AddDotNet();
            services.AddAwsCodeCommit();
            services.AddEmbeddedFileService();
            services.AddFindSolutionFile();

            services.AddSingletonIfNotExists<IUpdateResharperSettingsStrategy, UpdateLocalSolutionFile>();
        }
    }

    internal sealed class UpdateLocalSolutionFile(ConsoleService consoleService,
                                                  IGitService git,
                                                  IAwsCodeCommit awsCodeCommit,
                                                  FindSolutionFile findSolutionFile) : IUpdateResharperSettingsStrategy
    {
        public bool CanHandle(UpdateResharperSettingsParameters parameters)
        {
            return parameters.SolutionFile.IsNotNullOrWhiteSpace() &&
                   parameters.GitRepos.IsNullOrWhiteSpace();
        }

        public async Task HandleAsync(UpdateResharperSettingsParameters parameters)
        {
            // 0. Check that precondition is met
            if (CanHandle(parameters).IsFalse())
            {
                throw new RunJitException($"Please call {nameof(IUpdateResharperSettingsStrategy.CanHandle)} before call {nameof(IUpdateResharperSettingsStrategy.HandleAsync)}");
            }

            // 1. Check if solution file is the file or directory
            //    if it is null or whitespace we check current directory
            var solutionFile = findSolutionFile.Find(parameters.SolutionFile);

            // 2. Set current directory to solution file directory - cause of git commands and more
            Environment.CurrentDirectory = solutionFile.Directory!.FullName;

            // 3. Create new branch - only if git exists
            var branchName = "quality/update-resharper-settings";

            // 4. Check if git exists
            var existingGitFolder = solutionFile.Directory!.EnumerateDirectories(".git").FirstOrDefault();

            if (existingGitFolder.IsNotNull())
            {
                // NEW check for legacy branches and delete them all
                var branches = await git.GetRemoteBranchesAsync().ConfigureAwait(false);
                var legacyBranches = branches.Where(b => b.Name.Contains(branchName, StringComparison.OrdinalIgnoreCase)).ToImmutableList();

                await git.DeleteBranchesAsync(legacyBranches).ConfigureAwait(false);

                await git.CreateBranchAsync(branchName).ConfigureAwait(false);
            }

            var solutionName = solutionFile.NameWithoutExtension();

            var resharperSettings = EmbeddedFile.GetFileContentFrom("Update.ResharperSettings.Template.Resharper.sln.DotSettings");
            var resharperSettingsFile = new FileInfo(Path.Combine(solutionFile.Directory!.FullName, $"{solutionName}.sln.DotSettings"));

            if (resharperSettingsFile.Exists)
            {
                var existingFileContent = await File.ReadAllTextAsync(resharperSettingsFile.FullName).ConfigureAwait(false);

                if (resharperSettings.Length.EqualsTo(existingFileContent.Length))
                {
                    consoleService.WriteSuccess($"Solution: {solutionFile.FullName} R# setting already up to date nothing to update !");

                    return;
                }
            }

            await File.WriteAllTextAsync(resharperSettingsFile.FullName, resharperSettings).ConfigureAwait(false);

            if (existingGitFolder.IsNotNull())
            {
                // 9. Add changes to git
                await git.AddAsync().ConfigureAwait(false);

                // 10. Commit changes
                await git.CommitAsync("Update R# settings").ConfigureAwait(false);

                // 11. Push changes
                await git.PushAsync(branchName).ConfigureAwait(false);

                // 12. Create pull request
                await awsCodeCommit.CreatePullRequestAsync("Update R# settings",
                                                           "Update R# settings to the newest versions",
                                                           branchName).ConfigureAwait(false);
            }

            consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was successfully update to the newest Resharper settings");
        }
    }
}
