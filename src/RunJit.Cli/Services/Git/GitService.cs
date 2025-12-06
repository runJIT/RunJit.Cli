using System.Collections.Immutable;
using System.Text;
using Argument.Check;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using Process = DotNetTool.Service.Process;

namespace RunJit.Cli.Services.Git
{
    internal static class AddGitServiceExtension
    {
        internal static void AddGitService(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IGitService, GitService>();
        }
    }

    // I need an interface for all common git commands like.
    // git clone url as parameter
    // git checkout master as parameter
    // git reset --hard
    // git new branch name as parameter
    // git add .
    // git commit -m "message"
    // all methods should be async
    public interface IGitService
    {
        Task CloneAsync(string url,
                        string branchName = "");

        Task CloneAsync(string url,
                        DirectoryInfo targetFolder);

        Task CloneAsync(string url,
                        string branchName,
                        DirectoryInfo targetFolder);

        Task CheckoutAsync(string branchName);

        Task ResetHardAsync();

        Task CreateBranchAsync(string branchName);

        Task<bool> BranchExistAsync(string branchName);

        Task AddAsync();

        Task CommitAsync(string message);

        Task<string> GetOriginAsync();

        Task PullAsync();

        Task PushAsync(string branchName);

        Task<ImmutableList<BranchInfo>> ListBranchesAsync();

        Task DeleteBranchesAsync(ImmutableList<BranchInfo> branches);

        Task<ImmutableList<BranchInfo>> GetRemoteBranchesAsync();

        Task InitAsync();

        Task<ImmutableList<BranchInfo>> GetAllBranchesAsync();

        Task<ImmutableList<BranchInfo>> GetLocalBranchesAsync();

        Task FetchAsync();
    }

    public record BranchInfo(string Name,
                             bool IsActiveBranch);

    internal sealed class GitService(ConsoleService consoleService) : IGitService
    {
        public Task CloneAsync(string url,
                               string branchName = "")
        {
            var branchInfo = branchName.IsNullOrWhiteSpace() ? string.Empty : $" --branch {branchName}";

            return RunGitCommandAsync($"clone {branchInfo} {url}");
        }

        public Task CloneAsync(string url,
                               string branchName,
                               DirectoryInfo targetFolder)
        {
            Throw.IfNotExists(targetFolder);

            return RunGitCommandAsync($"clone -b {branchName} {url} {targetFolder.FullName}");
        }

        public Task CloneAsync(string url,
                               DirectoryInfo targetFolder)
        {
            Throw.IfNotExists(targetFolder);

            return RunGitCommandAsync($"clone {url} {targetFolder.FullName}");
        }

        public Task CheckoutAsync(string branchName)
        {
            return RunGitCommandAsync(@$"checkout ""{branchName}""");
        }

        public Task ResetHardAsync()
        {
            return RunGitCommandAsync("reset --hard");
        }

        public Task CreateBranchAsync(string branchName)
        {
            return RunGitCommandAsync($@"checkout -b ""{branchName}""");
        }

        public async Task<bool> BranchExistAsync(string branchName)
        {
            var allBranches = await ListBranchesAsync().ConfigureAwait(false);

            return allBranches.Any(b => b.Name.ToLowerInvariant().EqualsTo(branchName.ToLowerInvariant()));
        }

        public Task AddAsync()
        {
            return RunGitCommandAsync("add .");
        }

        public Task CommitAsync(string message)
        {
            return RunGitCommandAsync($@"commit -m ""{message}""");
        }

        public async Task<string> GetOriginAsync()
        {
            var result = await RunGitCommandAsync("config --get remote.origin.url").ConfigureAwait(false);

            return result.Split(Environment.NewLine).First().Replace("\n", string.Empty);
        }

        public Task PullAsync()
        {
            return RunGitCommandAsync("pull");
        }

        public Task PushAsync(string branchName)
        {
            return RunGitCommandAsync($@"push origin ""{branchName}""");
        }

        public async Task<ImmutableList<BranchInfo>> ListBranchesAsync()
        {
            var listBranchOutput = await RunGitCommandAsync("branch -r").ConfigureAwait(false);
            var splitResult = listBranchOutput.Split(Environment.NewLine);

            var branches = splitResult.Select(x => x.Trim())
                                      .Where(x => x.IsNotNullOrWhiteSpace())
                                      .Select(x =>
                                              {
                                                  var isActiveBranch = x.StartsWith("*");
                                                  var trimmed = x.Trim('*').TrimStart();

                                                  return new BranchInfo(trimmed, isActiveBranch);
                                              })
                                      .ToImmutableList();

            return branches;
        }

        public async Task<ImmutableList<BranchInfo>> GetAllBranchesAsync()
        {
            var listBranchOutput = await RunGitCommandAsync("branch -a").ConfigureAwait(false);
            var splitResult = listBranchOutput.Split(Environment.NewLine);
            var branchNames = splitResult.Select(x => x.Replace("origin/", string.Empty)).Select(x => x.Trim()).Where(x => x.IsNotNullOrWhiteSpace()).ToList();

            var activeBranch = branchNames.FirstOrDefault(b => b.StartWith("*"));

            var branches = branchNames.Select(b => new BranchInfo(b.Split("* ").Last(), b.EqualsTo(activeBranch)))
                                      .ToImmutableList();

            return branches;
        }

        public async Task<ImmutableList<BranchInfo>> GetLocalBranchesAsync()
        {
            var listBranchOutput = await RunGitCommandAsync("branch").ConfigureAwait(false);
            var splitResult = listBranchOutput.Split(Environment.NewLine);
            var branchNames = splitResult.Select(x => x.Replace("origin/", string.Empty)).Select(x => x.Trim()).Where(x => x.IsNotNullOrWhiteSpace()).ToList();

            var activeBranch = branchNames.FirstOrDefault(b => b.StartWith("*"));

            var branches = branchNames.Select(b => new BranchInfo(b.Split("* ").Last(), b.EqualsTo(activeBranch)))
                                      .ToImmutableList();

            return branches;
        }

        public Task FetchAsync()
        {
            return RunGitCommandAsync("fetch -p");
        }

        public async Task<ImmutableList<BranchInfo>> GetRemoteBranchesAsync()
        {
            var listBranchOutput = await RunGitCommandAsync("branch -r").ConfigureAwait(false);
            var splitResult = listBranchOutput.Split(Environment.NewLine);
            var branchNames = splitResult.Select(x => x.Replace("origin/", string.Empty)).Select(x => x.Trim()).Where(x => x.IsNotNullOrWhiteSpace()).ToList();

            var headbranch = branchNames.FirstOrDefault(b => b.Contains("head", StringComparison.OrdinalIgnoreCase));
            var headBranchName = headbranch?.Split("-> ").LastOrDefault();

            var branches = branchNames.Select(b => new BranchInfo(b, b.EqualsTo(headBranchName)))
                                      .ToImmutableList();

            return branches;
        }

        public Task InitAsync()
        {
            return RunGitCommandAsync("init");
        }

        public async Task DeleteBranchesAsync(ImmutableList<BranchInfo> branches)
        {
            foreach (var branchInfo in branches)
            {
                await RunGitCommandAsync($"push origin --delete {branchInfo.Name}").ConfigureAwait(false);
            }
        }

        private async Task<string> RunGitCommandAsync(string arguments)
        {
            consoleService.WriteInfo($"git {arguments}");

            var outputStringBuilder = new StringBuilder();
            var errorStringBuilder = new StringBuilder();

            var command = Process.StartProcess("git", arguments, null,
                                               o => outputStringBuilder.AppendLine(o), e => errorStringBuilder.AppendLine(e));

            await command.WaitForExitAsync().ConfigureAwait(false);

            var output = outputStringBuilder.ToString();
            var errors = errorStringBuilder.ToString();

            // var command = await dotNetTool.RunAsync("git", arguments).ConfigureAwait(false);
            if (command.ExitCode != 0)
            {
                if (output.Contains("nothing to commit, working tree clean").IsFalse())
                {
                    throw new RunJitException($"git {arguments} was not successful.: ErrorCode: {command.ExitCode}{Environment.NewLine}Output: {Environment.NewLine}{output}. Error:{errors}");
                }
            }

            consoleService.WriteSuccess($"git {arguments} was successful.");

            return output;
        }
    }
}
