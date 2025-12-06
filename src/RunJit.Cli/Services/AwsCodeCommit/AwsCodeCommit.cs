using System.Collections.Immutable;
using System.Text;
using Azure.Storage.Blobs.Models;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services.Git;
using Process = DotNetTool.Service.Process;

namespace RunJit.Cli.Services.AwsCodeCommit
{
    internal static class AddAwsCodeCommitExtension
    {
        internal static void AddAwsCodeCommit(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IAwsCodeCommit, AwsCodeCommit>();
        }
    }

    public interface IAwsCodeCommit
    {
        Task<PullRequestInfo> CreatePullRequestAsync(string title,
                                                     string description,
                                                     string sourceBranchName,
                                                     string targetBranchName = "master");

        Task<PullRequestInfo> CreatePullRequestAsync(string title,
                                                     string description,
                                                     string repositoryName,
                                                     string sourceBranchName,
                                                     string targetBranchName = "master");
    }

    internal sealed class AwsCodeCommit(ConsoleService consoleService,
                                        IGitService git) : IAwsCodeCommit
    {
        public async Task<PullRequestInfo> CreatePullRequestAsync(string title,
                                                                  string description,
                                                                  string sourceBranchName,
                                                                  string targetBranchName = "master")
        {
            var gitOrigin = await git.GetOriginAsync().ConfigureAwait(false);
            var repoName = gitOrigin.Split("//").Last();

            return await CreatePullRequestAsync(title,
                                                description,
                                                repoName,
                                                sourceBranchName,
                                                targetBranchName).ConfigureAwait(false);
        }

        public async Task<PullRequestInfo> CreatePullRequestAsync(string title,
                                                                  string description,
                                                                  string repositoryName,
                                                                  string sourceBranchName,
                                                                  string targetBranchName = "master")
        {
            consoleService.WriteInfo("Create AWS Code commit PR");
            consoleService.WriteInfo($@"aws codecommit create-pull-request --title ""{title}"" --description ""{description}"" --targets repositoryName=""{repositoryName},sourceReference={sourceBranchName},destinationReference={targetBranchName}""");

            var stringBuilder = new StringBuilder();
            var pullRequestResponse = Process.StartProcess("aws", $@"codecommit create-pull-request --title ""{title}"" --description ""{description}"" --targets repositoryName=""{repositoryName},sourceReference={sourceBranchName},destinationReference={targetBranchName}""", stdOut: s => stringBuilder.AppendLine(s));
            await pullRequestResponse.WaitForExitAsync().ConfigureAwait(false);

            var output = stringBuilder.ToString();

            // var pullRequestResponse = await donDotNetTool.RunAsync("aws", $@"codecommit create-pull-request --title ""{title}"" --description ""{description}"" --targets repositoryName=""{repositoryName},sourceReference={sourceBranchName},destinationReference={targetBranchName}""").ConfigureAwait(false);
            if (pullRequestResponse.ExitCode != 0 &&
                pullRequestResponse.ExitCode != 255) // 255 is creating PR successfully :/ strange
            {
                consoleService.WriteError($"Failed to create pull request. Exit code: {pullRequestResponse.ExitCode}. Message: {output}");
            }

            var pullrequestInfo = output.FromJsonStringAs<PullRequestResponse>();

            var target = pullrequestInfo.PullRequest.PullRequestTargets.First();
            var url = $"https://eu-central-1.console.aws.amazon.com/codesuite/codecommit/repositories/{target.RepositoryName}/pull-requests/{pullrequestInfo.PullRequest.PullRequestId}/details?region=eu-central-1";

            consoleService.WriteSuccess("AWS Code Commit pull request successfully created");

            return new PullRequestInfo() { AbsoluteUrl = url };
        }
    }

    public record PullRequestInfo
    {
        public string AbsoluteUrl { get; init; } = string.Empty;
    }

    public record PullRequestResponse
    {
        public PullRequest PullRequest { get; init; } = new PullRequest();
    }

    public record PullRequest
    {
        public string PullRequestId { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public DateTime LastActivityDate { get; init; }

        public DateTime CreationDate { get; init; }

        public string PullRequestStatus { get; init; } = string.Empty;

        public string AuthorArn { get; init; } = string.Empty;

        public ImmutableList<PullRequestTarget> PullRequestTargets { get; init; } = ImmutableList<PullRequestTarget>.Empty;

        public string ClientRequestToken { get; init; } = string.Empty;

        public string RevisionId { get; init; } = string.Empty;

        public ImmutableList<ApprovalRule> ApprovalRules { get; init; } = ImmutableList<ApprovalRule>.Empty;
    }

    public record PullRequestTarget
    {
        public string RepositoryName { get; init; } = string.Empty;

        public string SourceReference { get; init; } = string.Empty;

        public string DestinationReference { get; init; } = string.Empty;

        public string DestinationCommit { get; init; } = string.Empty;

        public string SourceCommit { get; init; } = string.Empty;

        public string MergeBase { get; init; } = string.Empty;

        public MergeMetadata MergeMetadata { get; init; } = new MergeMetadata();

        public string AbsolutUrl { get; init; } = string.Empty;
    }

    public record MergeMetadata
    {
        public bool IsMerged { get; init; }
    }

    public record ApprovalRule;
}
