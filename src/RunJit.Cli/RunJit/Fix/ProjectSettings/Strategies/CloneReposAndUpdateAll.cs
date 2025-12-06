using System.Collections.Immutable;
using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;
using RunJit.Cli.Services.AwsCodeCommit;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;
using Solution.Parser.Solution;

namespace RunJit.Cli.Fix.ProjectSettings
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

            services.AddSingletonIfNotExists<IFixProjectSettingsStrategy, CloneReposAndUpdateAll>();
        }
    }

    internal sealed class CloneReposAndUpdateAll(ConsoleService consoleService,
                                                 IGitService git,
                                                 IDotNet dotNet,
                                                 IAwsCodeCommit awsCodeCommit,
                                                 FindSolutionFile findSolutionFile) : IFixProjectSettingsStrategy
    {
        public bool CanHandle(FixProjectSettingsParameters parameters)
        {
            return parameters.SolutionFile.IsNullOrWhiteSpace() &&
                   parameters.GitRepos.IsNotNullOrWhiteSpace();
        }

        public async Task HandleAsync(FixProjectSettingsParameters parameters)
        {
            // 0. Check that precondition is met
            if (CanHandle(parameters).IsFalse())
            {
                throw new RunJitException($"Please call {nameof(IFixProjectSettingsStrategy.CanHandle)} before call {nameof(IFixProjectSettingsStrategy.HandleAsync)}");
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
                consoleService.WriteSuccess($"Start fixing project files for repo {index} of {repos.Length}");

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
                var branchName = "quality/fix-project-files";

                var legacyBranches = branches.Where(b => b.Name.Contains(branchName, StringComparison.OrdinalIgnoreCase)).ToImmutableList();

                await git.DeleteBranchesAsync(legacyBranches).ConfigureAwait(false);

                // 4. Create new branch, check that branch is unique
                var qualityFixProjectSettingsPackages = branchName;

                await git.CreateBranchAsync(qualityFixProjectSettingsPackages).ConfigureAwait(false);

                // 5. Check if solution file is the file or directory
                //    if it is null or whitespace we check current directory 
                var solutionFile = findSolutionFile.Find(Environment.CurrentDirectory);

                var parsedSolutionFile = new SolutionFileInfo(solutionFile.FullName).Parse();

                // All test project files needs
                // <!--Test project specific area-->
                // <PropertyGroup>
                //   <IsPackable>false</IsPackable>
                //   <IsPublishable>false</IsPublishable>
                //   <IsTestProject>true</IsTestProject>
                // </PropertyGroup>
                foreach (var testProject in parsedSolutionFile.UnitTestProjects)
                {
                    // 1. Remove all existing once if they exist but not in the correct area
                    var elements = new[] { "IsPackable", "IsPublishable", "IsTestProject" };

                    foreach (var elementName in elements)
                    {
                        var elementNode = testProject.Document.ElementsBy(elementName);

                        if (elementNode.IsNotNull())
                        {
                            elementNode.Remove();
                        }
                    }

                    // 2. Create a new item group for embedded files
                    //    <ItemGroup>
                    //        <EmbeddedResource Include="**\*.json" Exclude="bin\**\*;obj\**\*" />
                    //    </ItemGroup>
                    var toolEmbeddedFileSettingsComment = new XComment("Test project specific area");
                    var propertyGroup = new XElement("PropertyGroup");
                    var isPackable = new XElement("IsPackable");
                    isPackable.Value = "false";
                    var isPublishable = new XElement("IsPublishable");
                    isPublishable.Value = "false";

                    var isTestProject = new XElement("IsTestProject");
                    isTestProject.Value = "true";

                    propertyGroup.Add(isPackable);
                    propertyGroup.Add(isPublishable);
                    propertyGroup.Add(isTestProject);

                    // 3. Add the comment and new PropertyGroup to the root of the project file
                    testProject.Document.Root!.Add(toolEmbeddedFileSettingsComment, propertyGroup);

                    await File.WriteAllTextAsync(testProject.ProjectFileInfo.Value.FullName, testProject.Document.ToString());
                }

                // <!--Web-Api project specific-->
                // <PropertyGroup>
                //   <IsPackable>false</IsPackable>
                // </PropertyGroup>

                // <!--Library project specific area-->
                // <PropertyGroup>
                //   <IsPackable>true</IsPackable>
                //   <IsPublishable>false</IsPublishable>
                // </PropertyGroup>

                // <!--Lambda project specific area-->
                //<PropertyGroup>
                //    <IsPackable>false</IsPackable>
                //    <GenerateRuntimeConfigurationFiles>true</GenerateRuntimeConfigurationFiles>
                //    <AWSProjectType>Lambda</AWSProjectType>
                //    <!-- This property makes the build directory similar to a publish directory and helps the AWS .NET Lambda Mock Test Tool find project dependencies. -->
                //    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
                //    <!-- Generate ready to run images during publishing to improve cold start time. -->
                //    <PublishReadyToRun>true</PublishReadyToRun>
                //    <SelfContained>true</SelfContained>
                //    <ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>None</ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>
                //</PropertyGroup>

                // 6. Build the solution first
                await dotNet.BuildAsync(solutionFile).ConfigureAwait(false);

                // 7. Add changes to git
                await git.AddAsync().ConfigureAwait(false);

                // 8. Commit changes
                await git.CommitAsync("Fix projects files").ConfigureAwait(false);

                // 9. Push changes
                await git.PushAsync(qualityFixProjectSettingsPackages).ConfigureAwait(false);

                // 10. Create pull request
                await awsCodeCommit.CreatePullRequestAsync("Fix projects files",
                                                           "Fix projects files",
                                                           qualityFixProjectSettingsPackages).ConfigureAwait(false);

                consoleService.WriteSuccess($"Solution: {solutionFile.FullName}, all projects file was successfully fixed");
            }
        }
    }
}
