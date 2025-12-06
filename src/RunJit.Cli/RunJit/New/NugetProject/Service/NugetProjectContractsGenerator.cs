using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Generate.DotNetTool;
using RunJit.Cli.Services.Net;

namespace RunJit.Cli.New.NugetProject
{
    internal static class AddNugetProjectContractsGeneratorExtension
    {
        internal static void AddNugetProjectContractsGenerator(this IServiceCollection services)
        {
            // Project level code gens
            services.AddAppSettingsCodeGen();
            services.AddProgramCodeGen();

            services.AddSingletonIfNotExists<NugetProjectContractsGenerator>();
        }
    }

    internal sealed class NugetProjectContractsGenerator(IDotNet dotNet,
                                                         IEnumerable<INugetProjectContractsSpecificCodeGen> codeGenerators)

    {
        public async Task<FileInfo> GenerateAsync(Solution.Parser.Solution.SolutionFile solutionFile,
                                                  NugetProjectInfos minimalApiProjectInfos)
        {
            var solutionFileInfo = solutionFile.SolutionFileInfo.Value;

            // POC starts here
            // 1. Check if cli project already exists
            //    Depending on new restriction of microsoft we can not just check the .Net.Web.Sdk
            //    so we need to check the implementation
            var contractProject = solutionFile.ProductiveProjects.FirstOrDefault(p => p.ProjectFileInfo.FileNameWithoutExtenion.ToLowerInvariant().EqualsTo(minimalApiProjectInfos.ContractProjectName.ToLowerInvariant()));

            if (contractProject.IsNotNull())
            {
                // 1.1 Remove project
                await dotNet.RemoveProjectFromSolutionAsync(solutionFileInfo, contractProject.ProjectFileInfo.Value).ConfigureAwait(false);

                // 1.2 If exists remove all files
                contractProject.ProjectFileInfo.Value.Directory?.Delete(true);
            }

            var netToolFolder = new DirectoryInfo(Path.Combine(solutionFileInfo.Directory!.FullName, minimalApiProjectInfos.ContractProjectName));

            if (netToolFolder.Exists)
            {
                netToolFolder.Delete(true);
            }

            // 4. Create new console project
            // dotnet new console --output folder1/folder2/myapp
            var target = Path.Combine(solutionFileInfo.Directory!.FullName, "src", minimalApiProjectInfos.ContractProjectName);

            await dotNet.RunAsync("dotnet", $"new classlib --name {minimalApiProjectInfos.ContractProjectName} --output {target} --framework {minimalApiProjectInfos.NetVersion}").ConfigureAwait(false);

            // 5. Get the new created csproj
            var contractProjectFileInfo = new FileInfo(Path.Combine(target, $"{minimalApiProjectInfos.ContractProjectName}.csproj"));

            if (contractProjectFileInfo.NotExists())
            {
                throw new RunJitException($"Expected .Net library project does not exists. {contractProjectFileInfo.FullName}");
            }

            // 6. Add required nuget packages into project
            await dotNet.AddNugetPackageAsync(contractProjectFileInfo.FullName, "GitVersion.MsBuild", "5.12.0").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(contractProjectFileInfo.FullName, "Microsoft.Extensions.DependencyInjection", "9.0.3").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(contractProjectFileInfo.FullName, "Microsoft.Extensions.Configuration", "9.0.3").ConfigureAwait(false);

            // 7. Load csproj content to avoid multiple IO write actions to disk which cause io exceptions
            var xdocument = XDocument.Load(contractProjectFileInfo.FullName);

            // 8. Generate the whole command structure with arguments, options
            foreach (var codeGenerator in codeGenerators)
            {
                await codeGenerator.GenerateAsync(contractProjectFileInfo,
                                                  solutionFileInfo,
                                                  xdocument,
                                                  minimalApiProjectInfos).ConfigureAwait(false);
            }

            // 9. Save the modified csproj file just once to avoid multiple IO write actions to disk which cause io exceptions
            xdocument.Save(contractProjectFileInfo.FullName);

            // 10. And at least we add this project into the solution because we want to avoid to many refreshes as possible
            await dotNet.AddProjectToSolutionAsync(solutionFileInfo, contractProjectFileInfo, "NugetPackage").ConfigureAwait(false);

            // 11. Cleanup code to be in sync with target solution settings :)
            // await solutionCodeCleanup.CleanupSolutionAsync(solutionFileInfo).ConfigureAwait(false);

            // 12. Return the created csproj file
            return contractProjectFileInfo;
        }
    }
}
