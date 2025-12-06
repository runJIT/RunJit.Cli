using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;

namespace RunJit.Cli.RunJit.Update.BuildConfig
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

            services.AddSingletonIfNotExists<IUpdateBuildConfigStrategy, CloneReposAndUpdateAll>();
        }
    }

    internal sealed class CloneReposAndUpdateAll(ConsoleService consoleService,
                                                 IGitService git,
                                                 IDotNet dotNet,
                                                 IAwsCodeCommit awsCodeCommit,
                                                 FindSolutionFile findSolutionFile) : IUpdateBuildConfigStrategy
    {
        public bool CanHandle(UpdateBuildConfigParameters parameters)
        {
            return parameters.SolutionFile.IsNullOrWhiteSpace() &&
                   parameters.GitRepos.IsNotNullOrWhiteSpace();
        }

        public async Task HandleAsync(UpdateBuildConfigParameters parameters)
        {
            // 0. Check that precondition is met
            if (CanHandle(parameters).IsFalse())
            {
                throw new RunJitException($"Please call {nameof(IUpdateBuildConfigStrategy.CanHandle)} before call {nameof(IUpdateBuildConfigStrategy.HandleAsync)}");
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
                consoleService.WriteSuccess($"Start updating build configurations for repo {index} of {repos.Length}");

                Environment.CurrentDirectory = orginalStartFolder;

                // 1. Git clone
                await git.CloneAsync(repo).ConfigureAwait(false);

                // 2. Get created git folder
                var folder = repo.Split("//").Last();
                Environment.CurrentDirectory = Path.Combine(orginalStartFolder, folder);

                // 3. Checkout master branch
                await git.CheckoutAsync("master").ConfigureAwait(false);

                var branchName = "quality/update-buildconfig";

                // NEW check for legacy branches and delete them all
                var branches = await git.GetRemoteBranchesAsync().ConfigureAwait(false);
                var legacyBranches = branches.Where(b => b.Name.Contains(branchName, StringComparison.OrdinalIgnoreCase)).ToImmutableList();

                await git.DeleteBranchesAsync(legacyBranches).ConfigureAwait(false);

                // 4. Create new branch, check that branch is unique
                var qualityUpdateBuildConfigs = branchName;

                await git.CreateBranchAsync(qualityUpdateBuildConfigs).ConfigureAwait(false);

                // 5. Check if solution file is the file or directory
                //    if it is null or whitespace we check current directory 
                var solutionFile = findSolutionFile.Find(Environment.CurrentDirectory);

                // 6. Build the solution first
                await dotNet.BuildAsync(solutionFile).ConfigureAwait(false);

                // 8. Directory.Build.props
                var file = Path.Combine(solutionFile.Directory!.FullName, "Directory.Build.props");

                // 9. FileContent
                var content = EmbeddedFile.GetFileContentFrom("RunJit.Update.BuildConfig.Templates.Directory.Build.props");

                //10. Write directory.build.props
                await File.WriteAllTextAsync(file, content).ConfigureAwait(false);

                //12. Add changes to git
                await git.AddAsync().ConfigureAwait(false);

                //13. Commit changes
                await git.CommitAsync("Update build configurations").ConfigureAwait(false);

                //14. Push changes
                await git.PushAsync(qualityUpdateBuildConfigs).ConfigureAwait(false);

                //15. Create pull request
                await awsCodeCommit.CreatePullRequestAsync("Update build configurations",
                                                           "Update build configurations to the newest versions",
                                                           qualityUpdateBuildConfigs).ConfigureAwait(false);

                consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was successfully update to the newest nuget packages");
            }
        }
    }
}
