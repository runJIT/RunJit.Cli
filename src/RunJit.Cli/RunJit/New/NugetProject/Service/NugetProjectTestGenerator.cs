using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services.Net;
using Solution.Parser.Solution;

namespace RunJit.Cli.New.NugetProject
{
    internal static class AddNugetProjectTestGeneratorExtension
    {
        internal static void AddNugetProjectTestGenerator(this IServiceCollection services)
        {
            services.AddDotNet();
            services.AddEmbeddedFileSettings();
            services.AddRemoveDefaultTestClasses();

            services.AddSingletonIfNotExists<NugetProjectTestGenerator>();
        }
    }

    internal sealed class NugetProjectTestGenerator(IDotNet dotNet,
                                                  IEnumerable<INugetProjectTestSpecificCodeGen> codeGenerators)
    {
        internal async Task<FileInfo> GenerateAsync(SolutionFile solutionFile,
                                                    FileInfo webApiProjectFileInfo,
                                                    NugetProjectInfos minimalApiProjectInfos)
        {
            // 1. Get the solution file info
            var solutionFileInfo = solutionFile.SolutionFileInfo.Value;

            // 2. Expected test project
            var testProjectName = $"{minimalApiProjectInfos.ProjectName}.Test";
            var testProjectFileInfo = new FileInfo(Path.Combine(solutionFileInfo.Directory!.FullName, "src", testProjectName, $"{testProjectName}.csproj"));

            // 3. Check if cli test project already exists
            var dotNetToolTestProject = solutionFile.UnitTestProjects.FirstOrDefault(p => p.ProjectFileInfo.FileNameWithoutExtenion.ToLowerInvariant() == testProjectName.ToLowerInvariant());

            // 4. Important if a test project already exists we cant do a lot because some developers
            //    maybe already have changed some code which we would override
            if (dotNetToolTestProject.IsNull())
            {
                // Create new net tool test project
                // dotnet new mstest --output folder1/folder2/myapp
                await dotNet.RunAsync("dotnet", $"new mstest --output {testProjectFileInfo.Directory!.FullName} --framework {minimalApiProjectInfos.NetVersion}").ConfigureAwait(false);
            }
            else
            {
                // if already exists we get the real project file info because if could be csproj location is different as expected.
                // Sample ./src/nettool.test/nettool.test.csproj
                testProjectFileInfo = dotNetToolTestProject.ProjectFileInfo.Value;

                return testProjectFileInfo;
            }

            // 5. Get the new created csproj
            if (testProjectFileInfo.NotExists())
            {
                throw new RunJitException($"Expected .NetTool project does not exists. {testProjectFileInfo.FullName}");
            }

            //<ItemGroup>
            //    <PackageReference Include="AspNetCore.Simple.MsTest.Sdk" Version="6.0.8" />
            //    <PackageReference Include="DotNetTool.Service" Version="0.3.0" />
            //    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
            //    <PackageReference Include="MSTest" Version="3.10.4" />
            //    <PackageReference Include="MSTest.TestAdapter" Version="3.10.4" />
            //    <PackageReference Include="MSTest.TestFramework" Version="3.10.4" />
            //    </ItemGroup>
            
            // 6. Add required nuget packages into project
            await dotNet.AddNugetPackageAsync(testProjectFileInfo.FullName, "AspNetCore.Simple.MsTest.Sdk", "6.1.9").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(testProjectFileInfo.FullName, "Microsoft.NET.Test.Sdk", "17.14.1").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(testProjectFileInfo.FullName, "MSTest", "3.10.4").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(testProjectFileInfo.FullName, "MSTest.TestAdapter", "3.10.4").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(testProjectFileInfo.FullName, "MSTest.TestFramework", "3.10.4").ConfigureAwait(false);

            // 7. Add needed project references
            await dotNet.AddProjectReference(webApiProjectFileInfo, testProjectFileInfo).ConfigureAwait(false);

            // 8. Load csproj content to avoid multiple IO write actions to disk which cause io exceptions
            var xdocument = XDocument.Load(testProjectFileInfo.FullName);

            // 9. Generate the whole command structure with arguments, options
            //    We only generate any kind of code of the test project does not exist already
            //    otherwise we override the code from the developers
            if (dotNetToolTestProject.IsNull())
            {
                foreach (var codeGenerator in codeGenerators)
                {
                    await codeGenerator.GenerateAsync(testProjectFileInfo, solutionFileInfo, xdocument, minimalApiProjectInfos).ConfigureAwait(false);
                }
            }

            // 10. Save the modified csproj file just once to avoid multiple IO write actions to disk which cause io exceptions
            xdocument.Save(testProjectFileInfo.FullName);

            // 11. And at least we add this project into the solution because we want to avoid to many refreshes as possible
            await dotNet.AddProjectToSolutionAsync(solutionFileInfo, testProjectFileInfo, "NugetPackage").ConfigureAwait(false);

            // 12. Cleanup test code to be in sync with target solution settings :)
            // await solutionCodeCleanup.CleanupProjectAsync(solutionFileInfo, dotNetToolTestProjectFileInfo).ConfigureAwait(false);

            // 13. Return the created test csproj file
            return testProjectFileInfo;
        }
    }
}
