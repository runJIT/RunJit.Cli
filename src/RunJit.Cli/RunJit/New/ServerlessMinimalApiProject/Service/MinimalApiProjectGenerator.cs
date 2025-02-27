using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Generate.DotNetTool;
using RunJit.Cli.Services.Net;
using Solution.Parser.Solution;

namespace RunJit.Cli.New.MinimalApiProject
{
    internal static class AddMinimalApiProjectGeneratorExtension
    {
        internal static void AddMinimalApiProjectGenerator(this IServiceCollection services)
        {
            // Project level code gens
            services.AddAppSettingsCodeGen();
            services.AddProgramCodeGen();
            services.AddEmbeddedFileSettings();
            services.AddWriteEmbbededFileIntoTarget();
            services.AddProjectSolutionFolders();
            services.AddTestProjectSettings();

            services.AddSingletonIfNotExists<MinimalApiProjectGenerator>();
        }
    }

    public static class AddWriteEmbbededFileIntoTargetExtension
    {
        public static void AddWriteEmbbededFileIntoTarget(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IMinimalApiProjectSpecificCodeGen, WriteEmbbededFileIntoTarget>();
        }
    }

    internal sealed class WriteEmbbededFileIntoTarget : IMinimalApiProjectSpecificCodeGen
    {
        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        FileInfo solutionFile,
                                        XDocument projectDocument,
                                        MinimalApiProjectInfos minimalApiProjectInfos)
        {
            // Get all embedded files matching to the strcuture
            var embbededFiles = GetType().Assembly.GetManifestResourceNames();
            var webApiProjectResources = embbededFiles.Where(f => f.Contains("New.ServerlessMinimalApiProject.CodeGen.")).ToList();

            foreach (var webApiProjectResource in webApiProjectResources)
            {
                var fileExtension = Path.GetExtension(webApiProjectResource);
                var fileContent = EmbeddedFile.GetFileContentFrom(webApiProjectResource);

                var newFileContent = fileContent.Replace("$BasePath$", minimalApiProjectInfos.BasePath)
                                                .Replace("$ProjectName$", minimalApiProjectInfos.NormalizedName)
                                                .Replace("$Namespace$", minimalApiProjectInfos.NormalizedName);

                // Splitting at the double dot ".."
                var parts = webApiProjectResource.Split(["New.ServerlessMinimalApiProject.CodeGen."], StringSplitOptions.None);

                if (parts.Length == 2)
                {
                    // Replacing dots with backslashes in the file path part
                    var part = parts[1];
                    part = part.Replace("$ProjectName$", minimalApiProjectInfos.NormalizedName);

                    var isRelativePath = part.Contains(".github.") || part.Contains("src.");

                    if (isRelativePath.IsFalse())
                    {
                        var rootPathFile = Path.Combine(solutionFile.Directory!.FullName, part);
                        var rootPathFileInfo = new FileInfo(rootPathFile);

                        if (rootPathFileInfo.Directory!.NotExists())
                        {
                            rootPathFileInfo.Directory!.Create();
                        }

                        await File.WriteAllTextAsync(rootPathFileInfo.FullName, newFileContent).ConfigureAwait(false);

                        continue;
                    }

                    var nameWithoutExtension = solutionFile.NameWithoutExtension();
                    var normalizedPart = part.StartsWith("Project.") ? part.Replace("Project.", $"{nameWithoutExtension}.") : part;

                    var removeFileExtensions = part.Replace(fileExtension, string.Empty);

                    var transformedPath = isRelativePath ? removeFileExtensions.TrimStart('.').Replace('.', Path.DirectorySeparatorChar) : part;

                    // Concatenating the final path
                    // Special folders have to be carefully handled !
                    if (normalizedPart.StartsWith("."))
                    {
                        transformedPath = $".{transformedPath}";
                    }

                    var withFileExtension = $"{transformedPath.TrimEnd(Path.DirectorySeparatorChar)}{fileExtension}".Replace("..", ".");

                    var finalPath = Path.Combine(solutionFile.Directory!.FullName, withFileExtension.TrimStart(Path.DirectorySeparatorChar));

                    var projectNamePath = finalPath.Replace(@$"{Path.DirectorySeparatorChar}Project{Path.DirectorySeparatorChar}Test{Path.DirectorySeparatorChar}", $@"{Path.DirectorySeparatorChar}{minimalApiProjectInfos.NormalizedName}.Test{Path.DirectorySeparatorChar}")
                                                   .Replace(@$"{Path.DirectorySeparatorChar}Project{Path.DirectorySeparatorChar}", $@"{Path.DirectorySeparatorChar}{minimalApiProjectInfos.NormalizedName}{Path.DirectorySeparatorChar}");

                    var fileInfo = new FileInfo(projectNamePath);

                    if (fileInfo.Directory!.NotExists())
                    {
                        fileInfo.Directory!.Create();
                    }

                    await File.WriteAllTextAsync(fileInfo.FullName, newFileContent).ConfigureAwait(false);
                }
            }
        }
    }

    internal sealed class MinimalApiProjectGenerator(IDotNet dotNet,
                                              IEnumerable<IMinimalApiProjectSpecificCodeGen> codeGenerators)

    {
        public async Task<FileInfo> GenerateAsync(SolutionFile solutionFile,
                                                  MinimalApiProjectInfos minimalApiProjectInfos)
        {
            var solutionFileInfo = solutionFile.SolutionFileInfo.Value;

            // POC starts here
            // 1. Check if cli project already exists
            //    Depending on new restriction of microsoft we can not just check the .Net.Web.Sdk
            //    so we need to check the implementation
            var dotNetToolProject = solutionFile.ProductiveProjects.FirstOrDefault(p => p.ProjectFileInfo.FileNameWithoutExtenion.ToLowerInvariant() == minimalApiProjectInfos.ProjectName.ToLowerInvariant());

            if (dotNetToolProject.IsNotNull())
            {
                // 1.1 Remove project
                await dotNet.RemoveProjectFromSolutionAsync(solutionFileInfo, dotNetToolProject.ProjectFileInfo.Value).ConfigureAwait(false);

                // 1.2 If exists remove all files
                dotNetToolProject.ProjectFileInfo.Value.Directory?.Delete(true);
            }

            // 2. Create the .net tool folder -> the name of the tool
            var netToolFolder = new DirectoryInfo(Path.Combine(solutionFileInfo.Directory!.FullName, minimalApiProjectInfos.ProjectName));

            if (netToolFolder.Exists)
            {
                netToolFolder.Delete(true);
            }

            // 4. Create new console project
            // dotnet new console --output folder1/folder2/myapp
            var target = Path.Combine(solutionFileInfo.Directory!.FullName, "src", minimalApiProjectInfos.ProjectName);

            await dotNet.RunAsync("dotnet", $"new webapi --name {minimalApiProjectInfos.ProjectName} --output {target} --framework {minimalApiProjectInfos.NetVersion}").ConfigureAwait(false);

            // 5. Get the new created csproj
            var dotnetToolProject = new FileInfo(Path.Combine(target, $"{minimalApiProjectInfos.ProjectName}.csproj"));

            if (dotnetToolProject.NotExists())
            {
                throw new RunJitException($"Expected .NetTool project does not exists. {dotnetToolProject.FullName}");
            }

            // 6. Add required nuget packages into project
            // <PackageReference Include="AWSSDK.DynamoDBv2" Version="3.7.405.29" />
            await dotNet.AddNugetPackageAsync(dotnetToolProject.FullName, "AWSSDK.DynamoDBv2", "3.7.405.29").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotnetToolProject.FullName, "Asp.Versioning.Http", "8.1.0").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotnetToolProject.FullName, "Asp.Versioning.Mvc.ApiExplorer", "8.1.0").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotnetToolProject.FullName, "Amazon.Lambda.AspNetCoreServer.Hosting", "1.7.2").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotnetToolProject.FullName, "Extensions.Pack", "6.0.3").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotnetToolProject.FullName, "Microsoft.AspNetCore.Authentication.JwtBearer", "9.0.1").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotnetToolProject.FullName, "Siemens.AspNet.ErrorHandling", "2.1.0").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotnetToolProject.FullName, "AspNetCore.HealthChecks.UI", "9.0.0").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotnetToolProject.FullName, "AspNetCore.HealthChecks.UI.Client", "9.0.0").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotnetToolProject.FullName, "Microsoft.Extensions.Diagnostics.HealthChecks", "9.0.1").ConfigureAwait(false);
            await dotNet.AddNugetPackageAsync(dotnetToolProject.FullName, "Microsoft.AspNetCore.JsonPatch", "9.0.1").ConfigureAwait(false);

                                                                                   // 7. Load csproj content to avoid multiple IO write actions to disk which cause io exceptions
                                                                                   var xdocument = XDocument.Load(dotnetToolProject.FullName);

            // 8. Generate the whole command structure with arguments, options
            foreach (var codeGenerator in codeGenerators)
            {
                await codeGenerator.GenerateAsync(dotnetToolProject, solutionFileInfo, xdocument, minimalApiProjectInfos).ConfigureAwait(false);
            }

            // 9. Save the modified csproj file just once to avoid multiple IO write actions to disk which cause io exceptions
            xdocument.Save(dotnetToolProject.FullName);

            // 10. And at least we add this project into the solution because we want to avoid to many refreshes as possible
            await dotNet.AddProjectToSolutionAsync(solutionFileInfo, dotnetToolProject, "Api").ConfigureAwait(false);

            // 11. Cleanup code to be in sync with target solution settings :)
            // await solutionCodeCleanup.CleanupSolutionAsync(solutionFileInfo).ConfigureAwait(false);

            // 12. Return the created csproj file
            return dotnetToolProject;
        }
    }
}
