using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;

namespace RunJit.Cli.RunJit.Update.Nuget
{
    internal static class AddCloneReposAndUpdateAllExtension
    {
        internal static void AddCloneReposAndUpdateAll(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddGitService();
            services.AddDotNet();
            services.AddUpdateNugetPackageService();
            services.AddAwsCodeCommit();
            services.AddFindSolutionFile();

            services.AddSingletonIfNotExists<IUpdateNugetStrategy, CloneReposAndUpdateAll>();
        }
    }

    internal sealed class CloneReposAndUpdateAll(ConsoleService consoleService,
                                                 IGitService git,
                                                 IDotNet dotNet,
                                                 IUpdateNugetPackageService updateNugetPackageService,
                                                 IAwsCodeCommit awsCodeCommit,
                                                 FindSolutionFile findSolutionFile) : IUpdateNugetStrategy
    {
        public bool CanHandle(UpdateNugetParameters parameters)
        {
            return parameters.SolutionFile.IsNullOrWhiteSpace() &&
                   parameters.GitRepos.IsNotNullOrWhiteSpace();
        }

        public async Task HandleAsync(UpdateNugetParameters parameters)
        {
            // 0. Check that precondition is met
            if (CanHandle(parameters).IsFalse())
            {
                throw new RunJitException($"Please call {nameof(IUpdateNugetStrategy.CanHandle)} before call {nameof(IUpdateNugetStrategy.HandleAsync)}");
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
                consoleService.WriteSuccess($"Start Upgrading repo {index} of {repos.Length}");

                Environment.CurrentDirectory = orginalStartFolder;

                // 1. Git clone
                await git.CloneAsync(repo).ConfigureAwait(false);

                // 2. Get created git folder
                var folder = repo.Split("//").Last();
                Environment.CurrentDirectory = Path.Combine(orginalStartFolder, folder);

                // 3. Checkout master branch
                await git.CheckoutAsync("master").ConfigureAwait(false);

                var branchName = "quality/update-nuget-packages";

                // NEW check for legacy branches and delete them all
                var branches = await git.GetRemoteBranchesAsync().ConfigureAwait(false);
                var legacyBranches = branches.Where(b => b.Name.Contains(branchName, StringComparison.OrdinalIgnoreCase)).ToImmutableList();

                await git.DeleteBranchesAsync(legacyBranches).ConfigureAwait(false);

                // 4. Create new branch, check that branch is unique
                var qualityUpdateNugetPackages = branchName;

                await git.CreateBranchAsync(qualityUpdateNugetPackages).ConfigureAwait(false);

                // 5. Check if solution file is the file or directory
                //    if it is null or whitespace we check current directory 
                var solutionFile = findSolutionFile.Find(Environment.CurrentDirectory);

                // 6. Build the solution first
                await dotNet.BuildAsync(solutionFile).ConfigureAwait(false);

                // 7. Get infos which packages are outdated
                var outdatedNugetResponse = await dotNet.ListOutdatedPackagesAsync(solutionFile).ConfigureAwait(false);

                // 8. Update the nuget packages
                await updateNugetPackageService.UpdateNugetPackageAsync(outdatedNugetResponse, parameters.IgnorePackages.Split(";").ToImmutableList()).ConfigureAwait(false);

                // just for testing
                var testprojectFolders = solutionFile.Directory!.EnumerateDirectories("*.Test");

                foreach (var testprojectFolder in testprojectFolders)
                {
                    foreach (var csharprFile in testprojectFolder.EnumerateFiles("*.cs", SearchOption.AllDirectories))
                    {
                        if (csharprFile.FullName.Contains("/bin") ||
                            csharprFile.FullName.Contains("/obj"))
                        {
                            continue;
                        }

                        var content = await File.ReadAllTextAsync(csharprFile.FullName).ConfigureAwait(false);

                        if (content.Contains("() => "))
                        {
                            var newContent = content.Replace("() => ", string.Empty);
                            await File.WriteAllTextAsync(csharprFile.FullName, newContent).ConfigureAwait(false);
                        }
                    }
                }

                // 9. Add changes to git
                await git.AddAsync().ConfigureAwait(false);

                // 10. Commit changes
                await git.CommitAsync("Update nuget packages").ConfigureAwait(false);

                // 11. Push changes
                await git.PushAsync(qualityUpdateNugetPackages).ConfigureAwait(false);

                // 12. Create pull request
                await awsCodeCommit.CreatePullRequestAsync("Update nuget packages",
                                                           "Update nuget packages to the newest versions",
                                                           qualityUpdateNugetPackages).ConfigureAwait(false);

                consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was successfully update to the newest nuget packages");
            }
        }
    }
}
