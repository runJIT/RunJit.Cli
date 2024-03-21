﻿using System.Collections.Immutable;
using System.IO.Compression;
using System.Net.Http.Headers;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using GenJit.Api.Client;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Auth0;
using RunJit.Cli.AwsCodeCommit;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Git;
using RunJit.Cli.Net;
using RunJit.Cli.RunJit.Update.Backend.Net;
using RunJit.Cli.RunJit.Update.Backend.Nuget;
using RunJit.Cli.Services;

namespace RunJit.Cli.RunJit.Update.CodeRules
{
    internal static class AddUpdateLocalSolutionFileExtension
    {
        internal static void AddUpdateLocalSolutionFile(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConsoleService();
            services.AddGitService();
            services.AddDotNet();
            services.AddDotNetService();
            services.AddAwsCodeCommit();
            services.AddUpdateNugetPackageService();
            services.AddRenameFilesAndFolders();
            services.AddFindSolutionFile();
            
            
            services.AddGenJitApiClientFactory(configuration);
            services.AddGenJitApiClient();
            services.AddHttpClient();
            services.AddMediator();

            services.AddSingletonIfNotExists<IUpdateCodeRulesStrategy, UpdateLocalSolutionFile>();
        }
    }

    internal class UpdateLocalSolutionFile(IConsoleService consoleService,
                                           IGitService git,
                                           //IAwsCodeCommit awsCodeCommit,
                                           IDotNet dotNet,
                                           IUpdateNugetPackageService updateNugetPackageService,
                                           IRenameFilesAndFolders renameFilesAndFolders,
                                           FindSolutionFile findSolutionFile,
                                           IGenJitApiClientFactory genJitApiClientFactory,
                                           IHttpClientFactory httpClientFactory,
                                           IMediator mediator) : IUpdateCodeRulesStrategy
    {
        public bool CanHandle(UpdateCodeRulesParameters parameters)
        {
            return parameters.SolutionFile.IsNotNullOrWhiteSpace();
        }

        public async Task HandleAsync(UpdateCodeRulesParameters parameters)
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
                // NEW check for legacy branches and delete them all
                var branches = await git.GetRemoteBranchesAsync().ConfigureAwait(false);
                var legacyBranches = branches.Where(b => b.Name.Contains(branchName, StringComparison.OrdinalIgnoreCase)).ToImmutableList();

                await git.DeleteBranchesAsync(legacyBranches).ConfigureAwait(false);

                await git.CreateBranchAsync(branchName).ConfigureAwait(false);
            }
            
            var currentRepoEnvironment = solutionFile.Directory!.FullName;
            var solutionNameNormalized = solutionFile.NameWithoutExtension().Split('.').Select(part => part.FirstCharToUpper()).Flatten(".");

            // 6. Build the solution first
            await dotNet.BuildAsync(solutionFile).ConfigureAwait(false);

            // 7. Get infos which packages are outdated
            var outdatedNugetTargetSolution = await dotNet.ListOutdatedPackagesAsync(solutionFile).ConfigureAwait(false);

            // 8. Update the nuget packages
            await updateNugetPackageService.UpdateNugetPackageAsync(outdatedNugetTargetSolution, parameters.IgnorePackages.Split(";").ToImmutableList()).ConfigureAwait(false);

            // Create temp folder for fetching and renaming and using
            var combine = Path.Combine(solutionFile.Directory!.FullName, Guid.NewGuid().ToString().ToLowerInvariant());
            var tempFolder = new DirectoryInfo(combine);
            
            var auth = await mediator.SendAsync(new GetTokenByStorageCache()).ConfigureAwait(false);
            
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(auth.TokenType, auth.Token);
            var genjitClient = genJitApiClientFactory.CreateFrom(httpClient);
            
            var codeRuleAsFileStream = await genjitClient.CodeRules.V1.ExportCodeRulesAsync().ConfigureAwait(false);
            
            using var zipArchive = new ZipArchive(codeRuleAsFileStream.FileStream, ZipArchiveMode.Read);
            zipArchive.ExtractToDirectory(tempFolder.FullName);
            
            // 5. Check if solution file is the file or directory
            //    if it is null or whitespace we check current directory 
            var codeRuleSolution = findSolutionFile.Find(tempFolder.FullName);

            // 6. Build the solution first
            await dotNet.BuildAsync(codeRuleSolution).ConfigureAwait(false);

            // 7. Get infos which packages are outdated
            var outdatedNugetCodeRuleSolution = await dotNet.ListOutdatedPackagesAsync(codeRuleSolution).ConfigureAwait(false);

            // 8. Update the nuget packages
            await updateNugetPackageService.UpdateNugetPackageAsync(outdatedNugetCodeRuleSolution, parameters.IgnorePackages.Split(";").ToImmutableList()).ConfigureAwait(false);

            Environment.CurrentDirectory = currentRepoEnvironment;


            foreach (var file in tempFolder.EnumerateFiles("*.cs", SearchOption.AllDirectories))
            {
                var fileContent = await File.ReadAllTextAsync(file.FullName);
                if (fileContent.Contains("Pulse.CodeRules.sln"))
                {
                    var newFileContent = fileContent.Replace("Pulse.CodeRules.sln", $"{solutionNameNormalized}.sln");
                    await File.WriteAllTextAsync(file.FullName, newFileContent);
                }
            }

            var alreadyExistingCodeRuleFolder = solutionFile.Directory!.EnumerateDirectories("*.CodeRules").FirstOrDefault();
            if (alreadyExistingCodeRuleFolder.IsNotNull())
            {
                alreadyExistingCodeRuleFolder.Delete(true);
            }

            var codeRuleFolder = tempFolder.EnumerateDirectories("Pulse.CodeRules", SearchOption.AllDirectories).FirstOrDefault();
            if (codeRuleFolder.IsNull())
            {
                throw new FileNotFoundException($"Could not find the code rules folder Pulse.CodeRules");
            }

            var newTarget = new DirectoryInfo(Path.Combine(solutionFile.Directory!.FullName, codeRuleFolder.Name));
            codeRuleFolder.MoveTo(newTarget.FullName);

            var renamedDirectory = renameFilesAndFolders.Rename(newTarget, "Pulse.CodeRules", $"{solutionNameNormalized}.CodeRules");

            consoleService.WriteSuccess($"New code rule folder: {renamedDirectory.FullName}");

            var newCodeRuleProject = renamedDirectory.EnumerateFiles("*.csproj").FirstOrDefault();
            if (newCodeRuleProject.IsNull())
            {
                throw new FileNotFoundException($"Could not find the code rules project after renaming process.");
            }

            await dotNet.AddProjectToSolutionAsync(solutionFile, newCodeRuleProject);

            tempFolder.Delete(true);

            // 6. Build the solution first
            await dotNet.BuildAsync(solutionFile).ConfigureAwait(false);

            // 7. Get infos which packages are outdated
            // var outdatedCodeRulesResponse = await dotNet.ListOutdatedPackagesAsync(solutionFile).ConfigureAwait(false);

            // await updateCodeRulesPackageService.UpdateCodeRulesPackageAsync(outdatedCodeRulesResponse).ConfigureAwait(false);

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
            
            consoleService.WriteSuccess($"Solution: {solutionFile.FullName} was successfully update to the newest code rules");
        }
    }
}