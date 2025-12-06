using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.RunJit.Update.Nuget;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;

namespace RunJit.Cli.Update.Strategies
{
    internal static class AddCloneReposAndUpdateExtension
    {
        internal static void AddCloneReposAndUpdate(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddGitService();
            services.AddDotNet();
            services.AddUpdateNugetPackageService();
            services.AddAwsCodeCommit();
            services.AddFindSolutionFile();
            services.AddUpdateAllFilesService();

            services.AddSingletonIfNotExists<IUpdateDotNetVersionStrategy, CloneReposAndUpdate>();
        }
    }

    internal sealed class CloneReposAndUpdate(ConsoleService consoleService,
                                              IGitService git,
                                              IDotNet dotNet,
                                              IAwsCodeCommit awsCodeCommit,
                                              FindSolutionFile findSolutionFile,
                                              UpdateAllFilesService updateAllFilesService) : IUpdateDotNetVersionStrategy
    {
        public bool CanHandle(UpdateDotNetVersionParameters versionParameters)
        {
            return versionParameters.SolutionFile.IsNullOrWhiteSpace() &&
                   versionParameters.GitRepos.IsNotNullOrWhiteSpace();
        }

        public async Task HandleAsync(UpdateDotNetVersionParameters parameters)
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

                var branchName = $"quality/update-dotnet-{parameters.Version}";

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
                parameters = parameters with { SolutionFile = solutionFile.FullName };

                // 6. Build the solution first
                await dotNet.BuildAsync(solutionFile).ConfigureAwait(false);

                await updateAllFilesService.HandleAsync(parameters).ConfigureAwait(false);

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
