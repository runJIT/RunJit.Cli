using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;
using RunJit.Cli.Services.Resharper;

namespace RunJit.Cli.RunJit.Cleanup.Code
{
    internal static class AddCloneReposAndUpdateAllExtension
    {
        internal static void AddCloneReposAndUpdateAll(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddGitService();
            services.AddDotNet();
            services.AddAwsCodeCommit();
            services.AddFindSolutionFile();

            services.AddSingletonIfNotExists<ICleanupCodeStrategy, CloneReposAndUpdateAll>();
        }
    }

    internal sealed class CloneReposAndUpdateAll(ConsoleService consoleService,
                                                 IGitService git,
                                                 IDotNet dotNet,
                                                 IAwsCodeCommit awsCodeCommit,
                                                 FindSolutionFile findSolutionFile,
                                                 SolutionCodeCleanup solutionCodeCleanup) : ICleanupCodeStrategy
    {
        public bool CanHandle(CleanupCodeParameters parameters)
        {
            return parameters.SolutionFile.IsNullOrWhiteSpace() &&
                   parameters.GitRepos.IsNotNullOrWhiteSpace();
        }

        public async Task HandleAsync(CleanupCodeParameters parameters)
        {
            // 0. Check that precondition is met
            if (CanHandle(parameters).IsFalse())
            {
                throw new RunJitException($"Please call {nameof(ICleanupCodeStrategy.CanHandle)} before call {nameof(ICleanupCodeStrategy.HandleAsync)}");
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
                consoleService.WriteSuccess($"Start fixing embedded resources for repo {index} of {repos.Length}");

                Environment.CurrentDirectory = orginalStartFolder;

                // 1. Git clone
                await git.CloneAsync(repo).ConfigureAwait(false);

                // 2. Get created git folder
                var folder = repo.Split("//").Last();
                var currentRepoEnvironment = Path.Combine(orginalStartFolder, folder);
                Environment.CurrentDirectory = currentRepoEnvironment;

                // 3. Checkout master branch
                await git.CheckoutAsync("master").ConfigureAwait(false);

                // 4. NEW check for legacy branches and delete them all
                var branches = await git.GetRemoteBranchesAsync().ConfigureAwait(false);
                var branchName = "quality/cleanup-code";
                var legacyBranches = branches.Where(b => b.Name.Contains(branchName, StringComparison.OrdinalIgnoreCase)).ToImmutableList();
                await git.DeleteBranchesAsync(legacyBranches).ConfigureAwait(false);

                // 5. Create new branch, check that branch is unique
                var qualityCleanupCodePackages = branchName;
                await git.CreateBranchAsync(qualityCleanupCodePackages).ConfigureAwait(false);

                // 6. Check if solution file is the file or directory
                //    if it is null or whitespace we check current directory 
                var solutionFile = findSolutionFile.Find(Environment.CurrentDirectory);

                // 8. Build the solution first, we can not clean up the code if the solution is not building
                await dotNet.BuildAsync(solutionFile).ConfigureAwait(false);

                // 9. Run cleanup code
                await solutionCodeCleanup.CleanupSolutionAsync(solutionFile).ConfigureAwait(false);

                //10. Add changes to git
                await git.AddAsync().ConfigureAwait(false);

                //11. Commit changes
                await git.CommitAsync("Code cleanup and formating").ConfigureAwait(false);

                //12. Push changes
                await git.PushAsync(qualityCleanupCodePackages).ConfigureAwait(false);

                //13. Create pull request
                await awsCodeCommit.CreatePullRequestAsync("Code cleanup and formating",
                                                           "Code cleanup and formating",
                                                           qualityCleanupCodePackages).ConfigureAwait(false);

                consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was successfully cleaned up");
            }
        }
    }
}
