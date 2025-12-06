using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;
using RunJit.Cli.Services.Slack;
using SlackNet;
using SlackNet.WebApi;

namespace RunJit.Cli.Update.TargetPlatform
{
    internal static class AddCloneReposAndUpdateExtension
    {
        internal static void AddCloneReposAndUpdate(this IServiceCollection services,
                                                    IConfiguration configuration)
        {
            services.AddConsoleService();
            services.AddGitService();
            services.AddDotNet();
            services.AddAwsCodeCommit();
            services.AddFindSolutionFile();
            services.AddUpdateAllFilesService();
            services.AddSlackSettings(configuration);
            services.AddUpdateTargetPlatformLocal();
            services.AddPlatformProvider();

            services.AddSingletonIfNotExists<IUpdateTargetPlatformStrategy, CloneReposAndUpdate>();
        }
    }

    internal sealed class CloneReposAndUpdate(ConsoleService consoleService,
                                              IGitService git,
                                              IAwsCodeCommit awsCodeCommit,
                                              FindSolutionFile findSolutionFile,
                                              UpdateTargetPlatformLocal updateTargetPlatformLocal,
                                              PlatformProvider platformProvider,
                                              SlackSettings slackSettings) : IUpdateTargetPlatformStrategy
    {
        public bool CanHandle(UpdateTargetPlatformParameters versionParameters)
        {
            return versionParameters.SolutionFile.IsNullOrWhiteSpace() &&
                   versionParameters.GitRepos.IsNotNullOrWhiteSpace();
        }

        public async Task HandleAsync(UpdateTargetPlatformParameters parameters)
        {
            // 0. Check that precondition is met
            if (CanHandle(parameters).IsFalse())
            {
                throw new RunJitException($"Please call {nameof(IUpdateTargetPlatformStrategy.CanHandle)} before call {nameof(IUpdateTargetPlatformStrategy.HandleAsync)}");
            }

            var availablePlatforms = platformProvider.GetSupportedPlatforms();

            var matchingPlatform = availablePlatforms.FirstOrDefault(p => p.EqualsTo(parameters.Platform));

            if (matchingPlatform.IsNull())
            {
                throw new RunJitException($"Platform: {parameters.Platform} is not supported. Supported platforms are: {Environment.NewLine}{availablePlatforms.Flatten(Environment.NewLine)}");
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

                var branchName = $"quality/update-to-{matchingPlatform}";

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
                // await dotNet.BuildAsync(solutionFile).ConfigureAwait(false);

                await updateTargetPlatformLocal.HandleAsync(parameters).ConfigureAwait(false);

                // 9. Add changes to git
                await git.AddAsync().ConfigureAwait(false);

                // 10. Commit changes
                await git.CommitAsync($"Update target platform to: {matchingPlatform}").ConfigureAwait(false);

                // 11. Push changes
                await git.PushAsync(qualityUpdateNugetPackages).ConfigureAwait(false);

                // 12. Create pull request
                var pullRequest = await awsCodeCommit.CreatePullRequestAsync($"Update target platform to: {matchingPlatform}",
                                                                             $"Update target platform to: {matchingPlatform}",
                                                                             qualityUpdateNugetPackages).ConfigureAwait(false);

                var slackApiClient = new SlackServiceBuilder().UseApiToken(slackSettings.Token)
                                                              .GetApiClient();

                var moduleName = folder.Split("-").Select(name => name.FirstCharToUpper()).Flatten(" ");

                // Nachrichten aus dem Channel abrufen
                // var historyResponse = await slackApiClient.Conversations.History(slackSettings.PullRequestChannel.Id).ConfigureAwait(false);
                // var existingMessage = historyResponse.Messages.FirstOrDefault(m => m.Text.Contains($"{moduleName}: Update .Net version to:"));

                await slackApiClient.Chat.PostMessage(new Message
                                                      {
                                                          Text = $"{moduleName}: Upgrade target platfrom to: {parameters.Platform}{Environment.NewLine}<{pullRequest.AbsoluteUrl}>",
                                                          Channel = slackSettings.PullRequestChannel.Name
                                                      });

                consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was upgraded to deployment platform: {matchingPlatform}");
            }
        }
    }
}
