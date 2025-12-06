using System.Collections.Immutable;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Api.Client;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.RunJit.Update.Nuget;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;

namespace RunJit.Cli.RunJit.Update.CodeRules
{
    internal static class AddUpdateLocalSolutionFileExtension
    {
        internal static void AddUpdateLocalSolutionFile(this IServiceCollection services,
                                                        IConfiguration configuration)
        {
            services.AddConsoleService();
            services.AddGitService();
            services.AddDotNet();
            services.AddRenameFilesAndFolders();
            services.AddFindSolutionFile();
            services.AddAwsCodeCommit();
            services.AddUpdateNugetPackageService();
            services.AddRenameFilesAndFolders();

            services.AddRunJitApiClientFactory(configuration);
            services.AddRunJitApiClient();
            services.AddHttpClient();

            services.AddMediatR(cfg =>
                                {
                                    cfg.RegisterServicesFromAssembly(typeof(AddUpdateLocalSolutionFileExtension).Assembly);
                                });

            services.AddSingletonIfNotExists<IUpdateCodeRulesStrategy, UpdateLocalSolutionFile>();
        }
    }

    internal sealed class UpdateLocalSolutionFile(ConsoleService consoleService,
                                                  IGitService git,
                                                  IDotNet dotNet,
                                                  IRenameFilesAndFolders renameFilesAndFolders,
                                                  FindSolutionFile findSolutionFile) : IUpdateCodeRulesStrategy
    {
        public bool CanHandle(UpdateCodeRulesParameters parameters)
        {
            return parameters.SolutionFile.IsNotNullOrWhiteSpace();
        }

        public async Task HandleAsync(UpdateCodeRulesParameters parameters)
        {
            try
            {
                await HandleInternalAsync(parameters).ConfigureAwait(false);
            }
            catch (Exception)
            {
                var solutionFile = findSolutionFile.Find(parameters.SolutionFile);
                Cleanup(solutionFile);
            }
        }

        private async Task HandleInternalAsync(UpdateCodeRulesParameters parameters)
        {
            // 0. Check that precondition is met
            if (CanHandle(parameters).IsFalse())
            {
                throw new RunJitException($"Please call {nameof(IUpdateCodeRulesStrategy.CanHandle)} before call {nameof(IUpdateCodeRulesStrategy.HandleAsync)}");
            }

            // 5. Check if solution file is the file or directory
            //    if it is null or whitespace we check current directory 
            var solutionFile = findSolutionFile.Find(parameters.SolutionFile);

            var branchName = "quality/update-coderules";

            // 4. Check if git exists
            var existingGitFolder = solutionFile.Directory!.EnumerateDirectories(".git").FirstOrDefault();

            if (existingGitFolder.IsNotNull())
            {
                Environment.CurrentDirectory = solutionFile.Directory!.FullName;

                // NEW check for legacy branches and delete them all
                var branches = await git.GetRemoteBranchesAsync().ConfigureAwait(false);
                var localBranches = await git.GetLocalBranchesAsync().ConfigureAwait(false);

                if (localBranches.Any(b => b.IsActiveBranch && b.Name.EqualsTo(branchName)).IsFalse())
                {
                    var legacyBranches = branches.Where(b => b.Name.Contains(branchName, StringComparison.OrdinalIgnoreCase)).ToImmutableList();

                    await git.DeleteBranchesAsync(legacyBranches).ConfigureAwait(false);

                    await git.CreateBranchAsync(branchName).ConfigureAwait(false);
                }
            }

            var currentRepoEnvironment = solutionFile.Directory!.FullName;
            var solutionNameNormalized = solutionFile.NameWithoutExtension().Split('.').Select(part => part.FirstCharToUpper()).Flatten(".");

            // 6. Build the solution first
            // await dotNet.BuildAsync(solutionFile).ConfigureAwait(false);

            //// 7. Get infos which packages are outdated
            //var outdatedNugetTargetSolution = await dotNet.ListOutdatedPackagesAsync(solutionFile).ConfigureAwait(false);

            //// 8. Update the nuget packages
            //await updateNugetPackageService.UpdateNugetPackageAsync(outdatedNugetTargetSolution, parameters.IgnorePackages.Split(";").ToImmutableList()).ConfigureAwait(false);

            // Create temp folder for fetching and renaming and using
            var combine = Path.Combine(solutionFile.Directory!.FullName, Guid.NewGuid().ToString().ToLowerInvariant());
            var tempFolder = new DirectoryInfo(combine);

            //var auth = await mediator.SendAsync(new GetTokenByStorageCache()).ConfigureAwait(false);
            //var httpClient = httpClientFactory.CreateClient();
            //httpClient.BaseAddress = new Uri(runJitApiClientSettings.BaseAddress);
            //// httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(auth.TokenType, auth.Token);
            //var rRunJitApiClient = runJitApiClientFactory.CreateFrom(httpClient);

            //var codeRuleAsFileStream = await rRunJitApiClient.CodeRules.V1.ExportCodeRulesAsync().ConfigureAwait(false);
            using var codeRuleAsFileStream = this.GetType().Assembly.GetEmbeddedFileAsStream("RunJITCodeRules.zip");
            using var zipArchive = new ZipArchive(codeRuleAsFileStream, ZipArchiveMode.Read);
            zipArchive.ExtractToDirectory(tempFolder.FullName);

            Environment.CurrentDirectory = currentRepoEnvironment;

            //foreach (var file in tempFolder.EnumerateFiles("*.cs", SearchOption.AllDirectories))
            //{
            //    var fileContent = await File.ReadAllTextAsync(file.FullName);
            //    if (fileContent.Contains("RunJit.CodeRules.sln"))
            //    {
            //        var newFileContent = fileContent.Replace("RunJit.CodeRules.sln", $"{solutionNameNormalized}.sln");
            //        await File.WriteAllTextAsync(file.FullName, newFileContent);
            //    }
            //}

            var sourceFolder = solutionFile.Directory.EnumerateDirectories("src").FirstOrDefault();
            var targetFolder = sourceFolder ?? solutionFile.Directory;

            var alreadyExistingCodeRuleFolder = solutionFile.Directory!.EnumerateDirectories($"{solutionFile.NameWithoutExtension()}*.CodeRules").FirstOrDefault();

            if (alreadyExistingCodeRuleFolder.IsNotNull())
            {
                var projectFileInfo = alreadyExistingCodeRuleFolder.EnumerateFiles("*.csproj").FirstOrDefault();

                if (projectFileInfo.IsNotNull())
                {
                    await dotNet.RemoveProjectFromSolutionAsync(solutionFile, projectFileInfo).ConfigureAwait(false);
                }

                alreadyExistingCodeRuleFolder.Delete(true);
            }

            var fixtureFolder = solutionFile.Directory!.EnumerateDirectories($"{solutionFile.NameWithoutExtension()}*.Fixtures").FirstOrDefault();

            if (fixtureFolder.IsNotNull())
            {
                var projectFileInfo = fixtureFolder.EnumerateFiles("*.csproj").FirstOrDefault();

                if (projectFileInfo.IsNotNull())
                {
                    await dotNet.RemoveProjectFromSolutionAsync(solutionFile, projectFileInfo).ConfigureAwait(false);
                }

                fixtureFolder.Delete(true);
            }

            foreach (var directory in tempFolder.EnumerateDirectories())
            {
                // 1. Find solution file usage and exchange to correct solution file name
                var mstestbaseClasses = directory.EnumerateFiles("MsTestBase.cs", SearchOption.AllDirectories).ToList();

                foreach (var mstestbaseClass in mstestbaseClasses)
                {
                    var fileContent = await File.ReadAllTextAsync(mstestbaseClass.FullName).ConfigureAwait(false);

                    var pattern = @"new SolutionFileName\("".*\.sln""\)";
                    var replacement = @$"new SolutionFileName(""{solutionFile.Name}"")";
                    var result = Regex.Replace(fileContent, pattern, replacement);

                    await File.WriteAllTextAsync(mstestbaseClass.FullName, result).ConfigureAwait(false);
                }

                var newTarget = new DirectoryInfo(Path.Combine(targetFolder.FullName, directory.Name));

                if (newTarget.Exists)
                {
                    // If custom rules directory already exists skip it
                    if (newTarget.Name.Contains(".Custom", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    newTarget.Delete(true);
                }

                directory.MoveTo(newTarget.FullName);

                var projectNameFromCodeRules = directory.Name.Split(".").First();

                var renamedDirectory = renameFilesAndFolders.Rename(newTarget, projectNameFromCodeRules, solutionNameNormalized);

                consoleService.WriteSuccess($"New code rule folder: {renamedDirectory.FullName}");

                var newCodeRuleProject = renamedDirectory.EnumerateFiles("*.csproj").FirstOrDefault();

                if (newCodeRuleProject.IsNull())
                {
                    throw new FileNotFoundException("Could not find the code rules project after renaming process.");
                }
            }

            tempFolder.Delete(true);

            var allCodeRuleCsprojs = solutionFile.Directory.EnumerateFiles("*CodeRule*.csproj", SearchOption.AllDirectories)
                                                 .ToList();

            foreach (var allCodeRuleCsproj in allCodeRuleCsprojs)
            {
                await dotNet.AddProjectToSolutionAsync(solutionFile, allCodeRuleCsproj, "CodeRules").ConfigureAwait(false);
            }

            // 6. Build the solution first
            // await dotNet.BuildAsync(solutionFile).ConfigureAwait(false);

            // 7. Get infos which packages are outdated
            // var outdatedCodeRulesResponse = await dotNet.ListOutdatedPackagesAsync(solutionFile).ConfigureAwait(false);

            //  await updateCodeRulesPackageService.UpdateCodeRulesPackageAsync(outdatedCodeRulesResponse).ConfigureAwait(false);

            if (existingGitFolder.IsNotNull())
            {
                // 9.Add changes to git
                await git.AddAsync().ConfigureAwait(false);

                // 10. Commit changes
                await git.CommitAsync("Update coderules packages").ConfigureAwait(false);

                // 11. Push changes
                await git.PushAsync(branchName).ConfigureAwait(false);

                //// 12. Create pull request
                //await awsCodeCommit.CreatePullRequestAsync("Update coderules packages",
                //                                           "Update coderules packages to the newest versions",
                //                                           qualityUpdateCodeRulesPackages).ConfigureAwait(false);
            }

            Cleanup(solutionFile);

            consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was successfully update to the newest code rules");
        }

        private static void Cleanup(FileInfo solutionFile)
        {
            // Cleanup
            var folders = solutionFile.Directory!.EnumerateDirectories().ToImmutableList();

            foreach (var directoryInfo in folders)
            {
                if (Guid.TryParse(directoryInfo.Name, out _))
                {
                    directoryInfo.Delete(true);
                }

                if (directoryInfo.Name.Contains("RunJit", StringComparison.OrdinalIgnoreCase))
                {
                    directoryInfo.Delete(true);
                }
            }
        }
    }
}
