using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;
using RunJit.Cli.Update.TargetPlatform;
using SlackNet;
using SlackNet.WebApi;
using File = System.IO.File;

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
                                                 FindSolutionFile findSolutionFile,
                                                 SlackSettings slackSettings) : IUpdateGlobalJsonServiceStrategy
    {
        private const string GlobalJson = """
                                          {
                                            "sdk": {
                                              "version": "9.0.201",
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
            // Ensure that the parameters meet the required conditions before proceeding.
            if (CanHandle(parameters).IsFalse())
            {
                throw new RunJitException($"Please call {nameof(IUpdateGlobalJsonServiceStrategy.CanHandle)} before calling {nameof(IUpdateGlobalJsonServiceStrategy.HandleAsync)}");
            }

            // 1. Check if solution file is the file or directory
            // If the working directory is null or whitespace, use the current directory.
            var repos = parameters.GitRepos.Split(';');
            var orginalStartFolder = parameters.WorkingDirectory.IsNotNullOrWhiteSpace() ? parameters.WorkingDirectory : Environment.CurrentDirectory;

            // Ensure the working directory exists.
            if (Directory.Exists(orginalStartFolder) == false)
            {
                Directory.CreateDirectory(orginalStartFolder);
            }

            foreach (var repo in repos)
            {
                var index = repos.IndexOf(repo) + 1;
                consoleService.WriteSuccess($"Try checking if backends are build-able. Backend {index} of {repos.Length}");

                // Set the current directory to the original start folder.
                Environment.CurrentDirectory = orginalStartFolder;

                // 2. Git clone
                // Clone the repository into the working directory.
                await git.CloneAsync(repo).ConfigureAwait(false);

                // 3. Get created git folder
                // Extract the folder name from the repository URL and set it as the current directory.
                var folder = repo.Split("//").Last();
                var currentRepoEnvironment = Path.Combine(orginalStartFolder, folder);
                Environment.CurrentDirectory = currentRepoEnvironment;

                // 4. Checkout master branch
                // Switch to the master branch of the cloned repository.
                await git.CheckoutAsync("master").ConfigureAwait(false);

                // 5. Check for legacy branches and delete them all
                // Identify and remove any legacy branches related to the update process.
                var branches = await git.GetRemoteBranchesAsync().ConfigureAwait(false);
                var branchName = "quality/update-global-json";
                var legacyBranches = branches.Where(b => b.Name.Contains(branchName, StringComparison.OrdinalIgnoreCase)).ToImmutableList();
                await git.DeleteBranchesAsync(legacyBranches).ConfigureAwait(false);

                // 6. Create new branch, check that branch is unique
                // Create a new branch for the update process.
                var qualityCheckBackendBuildsPackages = branchName;
                await git.CreateBranchAsync(qualityCheckBackendBuildsPackages).ConfigureAwait(false);

                // 7. Check if solution file is the file or directory
                // Locate the solution file in the current directory.
                var solutionFile = findSolutionFile.Find(Environment.CurrentDirectory);

                // 8. Build the solution first
                // Attempt to build the solution to ensure it is functional.
                var tryBuildResult = await dotNet.TryBuildAsync(solutionFile).ConfigureAwait(false);

                // 9. Check if global.json exists and is up-to-date
                // Verify if the global.json file exists and matches the expected content.
                var globalJsonFilePath = Path.Combine(solutionFile.Directory!.FullName, "global.json");
                var globalJsonFile = new FileInfo(globalJsonFilePath);

                if (globalJsonFile.Exists)
                {
                    var globalJsonFileContent = await File.ReadAllTextAsync(globalJsonFile.FullName).ConfigureAwait(false);

                    if (globalJsonFileContent == GlobalJson)
                    {
                        consoleService.WriteSuccess("Global json already up to date, nothing to do");

                        return;
                    }
                }

                // Update the global.json file with the new content.
                await File.WriteAllTextAsync(globalJsonFile.FullName, GlobalJson);

                // 11. Add changes to git
                // Stage all changes for commit.
                await git.AddAsync().ConfigureAwait(false);

                // 12. Commit changes
                // Commit the changes with a message indicating the build failure.
                await git.CommitAsync("Update global.json").ConfigureAwait(false);

                // 13. Push changes
                // Push the committed changes to the remote repository.
                await git.PushAsync(qualityCheckBackendBuildsPackages).ConfigureAwait(false);

                // 14. Create pull request
                // Open a pull request to notify others of the build failure and request a review.
                var pullRequestInfo = await awsCodeCommit.CreatePullRequestAsync("Update of global json.",
                                                           "Update of global json.", qualityCheckBackendBuildsPackages).ConfigureAwait(false);

                
                var slackApiClient = new SlackServiceBuilder().UseApiToken(slackSettings.Token)
                                                              .GetApiClient();

                var moduleName = folder.Split("-").Select(name => name.FirstCharToUpper()).Flatten(" ");

                // Nachrichten aus dem Channel abrufen
                // var historyResponse = await slackApiClient.Conversations.History(slackSettings.PullRequestChannel.Id).ConfigureAwait(false);
                // var existingMessage = historyResponse.Messages.FirstOrDefault(m => m.Text.Contains($"{moduleName}: Update .Net version to:"));

                await slackApiClient.Chat.PostMessage(new Message
                                                      {
                                                          Text = $"{moduleName}: Update global.json{Environment.NewLine}<{pullRequestInfo.AbsoluteUrl}>",
                                                          Channel = slackSettings.PullRequestChannel.Name
                                                      });
                
                // Log success message for the processed solution.
                consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was successfully checked and was buildable");
            }
        }
    }
}
