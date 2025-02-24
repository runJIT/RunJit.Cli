using System.Collections.Immutable;
using Amazon.Runtime.Internal.Util;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;

namespace RunJit.Cli.Update.GlobalJson
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

            services.AddSingletonIfNotExists<IUpdateGlobalJsonServiceStrategy, CloneReposAndUpdateAll>();
        }
    }

    internal sealed class CloneReposAndUpdateAll(ConsoleService consoleService,
                                                 IGitService git,
                                                 IDotNet dotNet,
                                                 IAwsCodeCommit awsCodeCommit,
                                                 FindSolutionFile findSolutionFile) : IUpdateGlobalJsonServiceStrategy
    {
        private const string Globaljson = """
                                          {
                                            "sdk": {
                                              "version": "9.0.102",
                                              "rollForward": "disable"
                                            }
                                          }
                                          """;

        public bool CanHandle(UpdateGlobalJsonParameters parameters)
        {
            return parameters.SolutionFile.IsNullOrWhiteSpace() &&
                   parameters.GitRepos.IsNotNullOrWhiteSpace();
        }

        public async Task HandleAsync(UpdateGlobalJsonParameters parameters)
        {
            // 0. Check that precondition is met
            if (CanHandle(parameters).IsFalse())
            {
                throw new RunJitException($"Please call {nameof(IUpdateGlobalJsonServiceStrategy.CanHandle)} before call {nameof(IUpdateGlobalJsonServiceStrategy.HandleAsync)}");
            }

            // 1. Check if solution file is the file or directory
            //    if it is null or whitespace we check current directory
            var repos = parameters.GitRepos.Split(';');
            var orginalStartFolder = parameters.WorkingDirectory.IsNotNullOrWhiteSpace() ? parameters.WorkingDirectory : Environment.CurrentDirectory;

            if (Directory.Exists(orginalStartFolder) == false)
            {
                Directory.CreateDirectory(orginalStartFolder);
            }

            foreach (var repo in repos)
            {
                var index = repos.IndexOf(repo) + 1;
                consoleService.WriteSuccess($"Try checking if backends are build-able. Backend {index} of {repos.Length}");

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
                var branchName = "quality/update-global-json";

                var legacyBranches = branches.Where(b => b.Name.Contains(branchName, StringComparison.OrdinalIgnoreCase)).ToImmutableList();

                await git.DeleteBranchesAsync(legacyBranches).ConfigureAwait(false);

                // 4. Create new branch, check that branch is unique
                var qualityCheckBackendBuildsPackages = branchName;

                await git.CreateBranchAsync(qualityCheckBackendBuildsPackages).ConfigureAwait(false);

                // 5. Check if solution file is the file or directory
                //    if it is null or whitespace we check current directory 
                var solutionFile = findSolutionFile.Find(Environment.CurrentDirectory);

                // 6. Build the solution first
                var tryBuildResult = await dotNet.TryBuildAsync(solutionFile).ConfigureAwait(false);

                // if backend build fails, we need to fix it.
                if (tryBuildResult.WasSuccessful.IsFalse())
                {
                    // Create error txt -> hint pipeline can be green cause of different net versions !
                    var errorFile = Path.Combine(solutionFile.Directory!.FullName, "build-errors.txt");
                    await File.WriteAllTextAsync(errorFile, tryBuildResult.Message).ConfigureAwait(false);

                    // 7. Add changes to git
                    await git.AddAsync().ConfigureAwait(false);

                    // 8. Commit changes
                    await git.CommitAsync("Build failed").ConfigureAwait(false);

                    // 9. Push changes
                    await git.PushAsync(qualityCheckBackendBuildsPackages).ConfigureAwait(false);

                    // 10. Create pull request
                    await awsCodeCommit.CreatePullRequestAsync("Build failed please check.",
                                                               "Build failed. Please check the build errors.", qualityCheckBackendBuildsPackages).ConfigureAwait(false);
                }

                consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was successfully checked and was buildable");
            }
        }
    }
}
