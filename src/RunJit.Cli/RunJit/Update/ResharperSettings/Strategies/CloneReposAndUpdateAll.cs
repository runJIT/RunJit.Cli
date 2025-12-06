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
    internal static class AddCloneReposAndUpdateAllExtension
    {
        internal static void AddCloneReposAndUpdateAll(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddGitService();
            services.AddDotNet();
            services.AddAwsCodeCommit();
            services.AddEmbeddedFileService();
            services.AddFindSolutionFile();

            services.AddSingletonIfNotExists<IUpdateResharperSettingsStrategy, CloneReposAndUpdateAll>();
        }
    }

    internal sealed class CloneReposAndUpdateAll(ConsoleService consoleService,
                                                 IGitService git,
                                                 IAwsCodeCommit awsCodeCommit,
                                                 FindSolutionFile findSolutionFile) : IUpdateResharperSettingsStrategy
    {
        public bool CanHandle(UpdateResharperSettingsParameters parameters)
        {
            return parameters.SolutionFile.IsNullOrWhiteSpace() &&
                   parameters.GitRepos.IsNotNullOrWhiteSpace();
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
            var repos = parameters.GitRepos.Split(';');
            var orginalStartFolder = parameters.WorkingDirectory.IsNotNullOrWhiteSpace() ? parameters.WorkingDirectory : Environment.CurrentDirectory;

            if (Directory.Exists(orginalStartFolder).EqualsTo(false))
            {
                Directory.CreateDirectory(orginalStartFolder);
            }

            foreach (var repo in repos)
            {
                var index = repos.IndexOf(repo) + 1;
                consoleService.WriteSuccess($"Start upgrading resharper settings for repo {index} of {repos.Length}");

                Environment.CurrentDirectory = orginalStartFolder;

                // 1. Git clone
                await git.CloneAsync(repo).ConfigureAwait(false);

                // 2. Get created git folder
                var folder = repo.Split("//").Last();
                var currentRepoEnvironment = Path.Combine(orginalStartFolder, folder);
                Environment.CurrentDirectory = currentRepoEnvironment;

                // 3. Checkout master branch
                await git.CheckoutAsync("master").ConfigureAwait(false);

                // NEW check for legacy branches and delete them all
                var branches = await git.GetRemoteBranchesAsync().ConfigureAwait(false);
                var branchName = "quality/update-resharper-settings";

                var legacyBranches = branches.Where(b => b.Name.Contains(branchName, StringComparison.OrdinalIgnoreCase)).ToImmutableList();

                await git.DeleteBranchesAsync(legacyBranches).ConfigureAwait(false);

                // 4. Create new branch, check that branch is unique
                var qualityUpdateResharperSettingsPackages = branchName;

                // 5. Check if solution file is the file or directory
                //    if it is null or whitespace we check current directory 
                var solutionFile = findSolutionFile.Find(Environment.CurrentDirectory);

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

                await git.CreateBranchAsync(qualityUpdateResharperSettingsPackages).ConfigureAwait(false);

                await File.WriteAllTextAsync(resharperSettingsFile.FullName, resharperSettings).ConfigureAwait(false);

                // 9. Add changes to git
                await git.AddAsync().ConfigureAwait(false);

                // 10. Commit changes
                await git.CommitAsync("Update R# settings").ConfigureAwait(false);

                // 11. Push changes
                await git.PushAsync(qualityUpdateResharperSettingsPackages).ConfigureAwait(false);

                // 12. Create pull request
                await awsCodeCommit.CreatePullRequestAsync("Update R# settings",
                                                           "Update R# settings to the newest versions",
                                                           qualityUpdateResharperSettingsPackages).ConfigureAwait(false);

                consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was successfully update to the newest Resharper settings");
            }
        }
    }
}
