using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services.Net;
using Solution.Parser.Solution;

namespace RunJit.Cli.Generate.DotNetTool.DotNetTool.Test
{
    internal static class AddDotNetToolTestGeneratorExtension
    {
        internal static void AddDotNetToolTestGenerator(this IServiceCollection services)
        {
            services.AddCliRunnerCodeGen();
            services.AddGlobalSetupCodeGen();
            services.AddCommandStructureCodeGen();
            services.AddEnvironmentVariablesCodeGen();

            services.AddProjectEmbeddedFilesCodeGen();
            services.AddProjectSettingsCodeGen();
            services.AddGetHealthCheckTestCodeGen();
            services.AddRemoveDefaultTestClasses();

            services.AddSingletonIfNotExists<DotNetToolTestGenerator>();
        }
    }

    internal sealed class DotNetToolTestGenerator(IDotNet dotNet,
                                           IEnumerable<IDotNetToolTestSpecificCodeGen> codeGenerators)
    {
        internal async Task<FileInfo> GenerateAsync(SolutionFile solutionFile,
                                                    FileInfo netToolProject,
                                                    DotNetToolInfos dotNetToolInfos)
        {
            // 1. Get the solution file info
            var solutionFileInfo = solutionFile.SolutionFileInfo.Value;

            // 2. Expected test project
            //    New: We have to check if a 'src' folder exists
            var dotNetToolTestProjectName = $"{dotNetToolInfos.ProjectName}.Test";

            var sourceFolder = solutionFileInfo.Directory!.EnumerateDirectories("src").FirstOrDefault();
            var sourceFolderPart = sourceFolder.IsNull() ? string.Empty : sourceFolder.Name;

            var dotNetToolTestProjectFileInfo = new FileInfo(Path.Combine(solutionFileInfo.Directory!.FullName, sourceFolderPart, dotNetToolTestProjectName, $"{dotNetToolTestProjectName}.csproj"));

            // 3. Check if cli test project already exists
            var dotNetToolTestProject = solutionFile.UnitTestProjects.FirstOrDefault(p => p.ProjectFileInfo.FileNameWithoutExtenion.ToLowerInvariant() == dotNetToolTestProjectName.ToLowerInvariant());

            // Important if a test project already exists we cant do a lot because some developers
            // maybe already have changed some code which we would override
            if (dotNetToolTestProject.IsNull())
            {
                // Create new net tool test project
                // dotnet new mstest --output folder1/folder2/myapp
                await dotNet.RunAsync("dotnet", $"new mstest --output {dotNetToolTestProjectFileInfo.Directory!.FullName} --framework {dotNetToolInfos.NetVersion}").ConfigureAwait(false);
            }
            else
            {
                // if already exists we get the real project file info because if could be csproj location is different as expected.
                // Sample ./src/nettool.test/nettool.test.csproj
                dotNetToolTestProjectFileInfo = dotNetToolTestProject.ProjectFileInfo.Value;

                // Important if the .Net tool will be new generated it gets a new project guid so we have to unlink and relink later
                // await dotNet.RemoveProjectReference(netToolProject, dotNetToolTestProjectFileInfo).ConfigureAwait(false);

                // We only update the real new csproj reference then we complete
                // await dotNet.AddProjectReference(netToolProject, dotNetToolTestProjectFileInfo).ConfigureAwait(false);

                return dotNetToolTestProjectFileInfo;
            }

            // 4. Create the .net tool folder -> the name of the tool
            var netToolFolder = new DirectoryInfo(Path.Combine(dotNetToolTestProjectFileInfo.Directory!.FullName, dotNetToolInfos.NormalizedName));

            if (netToolFolder.Exists.IsFalse())
            {
                netToolFolder.Create();
            }

            // 5. Get the new created csproj
            if (dotNetToolTestProjectFileInfo.NotExists())
            {
                throw new RunJitException($"Expected .NetTool project does not exists. {dotNetToolTestProjectFileInfo.FullName}");
            }

            // 6. Add required nuget packages into project
            await dotNet.AddNugetPackageAsync(dotNetToolTestProjectFileInfo.FullName, "AspNetCore.Simple.MsTest.Sdk", "6.0.7").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotNetToolTestProjectFileInfo.FullName, "DotNetTool.Service", "0.3.0").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotNetToolTestProjectFileInfo.FullName, "Microsoft.NET.Test.Sdk", "17.13.0").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotNetToolTestProjectFileInfo.FullName, "MSTest", "3.8.2").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotNetToolTestProjectFileInfo.FullName, "MSTest.TestAdapter", "3.8.2").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotNetToolTestProjectFileInfo.FullName, "MSTest.TestFramework", "3.8.2").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotNetToolTestProjectFileInfo.FullName, "NSubstitute", "5.3.0").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotNetToolTestProjectFileInfo.FullName, "coverlet.collector", "6.0.4").ConfigureAwait(false);

            // 7. Add needed project references
            await dotNet.AddProjectReference(netToolProject, dotNetToolTestProjectFileInfo).ConfigureAwait(false);

            var webApiProject = solutionFile.ProductiveProjects.FirstOrDefault(p => p.ProjectFileInfo.FileNameWithoutExtenion == solutionFile.SolutionFileInfo.FileNameWithoutExtenion);
            if (webApiProject != null)
            {
                await dotNet.AddProjectReference(webApiProject.ProjectFileInfo.Value, dotNetToolTestProjectFileInfo).ConfigureAwait(false);
            }

            // 8. Load csproj content to avoid multiple IO write actions to disk which cause io exceptions
            var xdocument = XDocument.Load(dotNetToolTestProjectFileInfo.FullName);

            // 9. Generate the whole command structure with arguments, options
            //    We only generate any kind of code of the test project does not exist already
            //    otherwise we override the code from the developers
            if (dotNetToolTestProject.IsNull())
            {
                foreach (var codeGenerator in codeGenerators)
                {
                    await codeGenerator.GenerateAsync(dotNetToolTestProjectFileInfo, xdocument, dotNetToolInfos, webApiProject).ConfigureAwait(false);
                }
            }

            // 10. Save the modified csproj file just once to avoid multiple IO write actions to disk which cause io exceptions
            xdocument.Save(dotNetToolTestProjectFileInfo.FullName);

            // 11. And at least we add this project into the solution because we want to avoid to many refreshes as possible
            await dotNet.AddProjectToSolutionAsync(solutionFileInfo, dotNetToolTestProjectFileInfo, "Cli").ConfigureAwait(false);

            // 12. Cleanup test code to be in sync with target solution settings :)
            // await solutionCodeCleanup.CleanupProjectAsync(solutionFileInfo, dotNetToolTestProjectFileInfo).ConfigureAwait(false);

            // 13. Return the created test csproj file
            return dotNetToolTestProjectFileInfo;
        }
    }
}
