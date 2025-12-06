using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;
using Solution.Parser.Solution;

namespace RunJit.Cli.RunJit.Update.SwaggerTests
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

            services.AddSingletonIfNotExists<IUpdateSwaggerTestsStrategy, CloneReposAndUpdateAll>();
        }
    }

    internal sealed class CloneReposAndUpdateAll(ConsoleService consoleService,
                                                 IGitService git,
                                                 IDotNet dotNet,
                                                 IAwsCodeCommit awsCodeCommit,
                                                 EmbeddedFileService embeddedFileService,
                                                 FindSolutionFile findSolutionFile) : IUpdateSwaggerTestsStrategy
    {
        public bool CanHandle(UpdateSwaggerTestsParameters parameters)
        {
            return parameters.SolutionFile.IsNullOrWhiteSpace() &&
                   parameters.GitRepos.IsNotNullOrWhiteSpace();
        }

        public async Task HandleAsync(UpdateSwaggerTestsParameters parameters)
        {
            // 0. Check that precondition is met
            if (CanHandle(parameters).IsFalse())
            {
                throw new RunJitException($"Please call {nameof(IUpdateSwaggerTestsStrategy.CanHandle)} before call {nameof(IUpdateSwaggerTestsStrategy.HandleAsync)}");
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
                consoleService.WriteSuccess($"Start upgrading code rules for repo {index} of {repos.Length}");

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
                var branchName = "quality/update-swagger-tests";

                var legacyBranches = branches.Where(b => b.Name.Contains(branchName, StringComparison.OrdinalIgnoreCase)).ToImmutableList();

                await git.DeleteBranchesAsync(legacyBranches).ConfigureAwait(false);

                // 4. Create new branch, check that branch is unique
                var qualityUpdateSwaggerTestsPackages = branchName;

                await git.CreateBranchAsync(qualityUpdateSwaggerTestsPackages).ConfigureAwait(false);

                // 5. Check if solution file is the file or directory
                //    if it is null or whitespace we check current directory 
                var solutionFile = findSolutionFile.Find(Environment.CurrentDirectory);

                // 6. Build the solution first
                await dotNet.BuildAsync(solutionFile).ConfigureAwait(false);

                var solutionFileParsed = new SolutionFileInfo(solutionFile.FullName).Parse();

                var webApiProject = solutionFileParsed.ProductiveProjects.Where(p => p.Document.ToString().Contains("Sdk=\"Microsoft.NET.Sdk.Web\"")).ToImmutableList();

                if (webApiProject.Count != 1)
                {
                    throw new RunJitException($"Could not find a web api project in solution: {solutionFile.FullName}");
                }

                var targetTestProject = solutionFileParsed.UnitTestProjects.FirstOrDefault(p => p.ProjectFileInfo.FileNameWithoutExtenion.Contains($"{webApiProject[0].ProjectFileInfo.FileNameWithoutExtenion}.Test"));

                if (targetTestProject.IsNull())
                {
                    throw new RunJitException($"Could not find the test project for the web api in the solution: {solutionFile.FullName}");
                }

                // Go sure solution parser is in place
                // Now we start Magic
                var solutionParser = targetTestProject.PackageReferences.FirstOrDefault(p => p.Include.Contains("Solution.Parser"));

                if (solutionParser.IsNull())
                {
                    await dotNet.RunAsync("dotnet", $"add {targetTestProject.ProjectFileInfo.Value.FullName} package Solution.Parser").ConfigureAwait(false);
                }

                // JsonDiffPatch
                var jsonDiffPatch = targetTestProject.PackageReferences.FirstOrDefault(p => p.Include.Contains("JsonDiffPatch"));

                if (jsonDiffPatch.IsNull())
                {
                    await dotNet.RunAsync("dotnet", $"add {targetTestProject.ProjectFileInfo.Value.FullName} package JsonDiffPatch").ConfigureAwait(false);
                }

                var targetPath = Path.Combine(targetTestProject.ProjectFileInfo.Value.Directory!.FullName, "SwaggerInitTest.cs");
                var fileContent = EmbeddedFile.GetFileContentFrom("RunJit.Update.SwaggerTests.Templates.SwaggerTestInitializer.rps");
                var nameWithoutExtension = solutionFile.NameWithoutExtension().Split('.').Select(c => c.FirstCharToUpper()).Flatten(".");
                var withoutExtension = webApiProject[0].ProjectFileInfo.Value.NameWithoutExtension().Split('.').Select(c => c.FirstCharToUpper()).Flatten(".");

                fileContent = fileContent.Replace("$solutionName$", nameWithoutExtension)
                                         .Replace("$webApiProject$", withoutExtension);

                await File.WriteAllTextAsync(targetPath, fileContent).ConfigureAwait(false);

                //// Now we start Magic
                await dotNet.RunAsync("dotnet", "test --filter TestCategory=SwaggerInitializer").ConfigureAwait(false);

                // Now we have to embed the files if they was not already embedded
                var swaggerFolder = new DirectoryInfo(Path.Combine(targetTestProject.ProjectFileInfo.Value.Directory!.FullName, "Swagger"));
                var jsonFiles = swaggerFolder.EnumerateFiles("*.json", SearchOption.AllDirectories).ToImmutableList();

                // 7. Embed the JSON files into the test project
                foreach (var jsonFile in jsonFiles)
                {
                    var relativePath = jsonFile.FullName.Replace(targetTestProject.ProjectFileInfo.Value.Directory.Parent!.FullName, string.Empty).TrimStart('\\').Replace(@"\", ".").Replace(jsonFile.Name, string.Empty).TrimEnd('.');
                    embeddedFileService.EmbedFile(targetTestProject, relativePath, jsonFile.Name);
                }

                //// Now we delete the file :) like magic
                File.Delete(targetPath);

                // 9. Add changes to git
                await git.AddAsync().ConfigureAwait(false);

                // 10. Commit changes
                await git.CommitAsync("Update SwaggerTests packages").ConfigureAwait(false);

                // 11. Push changes
                await git.PushAsync(qualityUpdateSwaggerTestsPackages).ConfigureAwait(false);

                // 12. Create pull request
                await awsCodeCommit.CreatePullRequestAsync("Update SwaggerTests packages",
                                                           "Update SwaggerTests packages to the newest versions",
                                                           qualityUpdateSwaggerTestsPackages).ConfigureAwait(false);

                consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was successfully update to the newest SwaggerTests packages");
            }
        }
    }
}
