using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.New.NugetProject
{
    public static class AddWriteEmbbededFileIntoTargetExtension
    {
        public static void AddWriteEmbbededFileIntoTarget(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<INugetProjectSpecificCodeGen, WriteEmbbededFileIntoTarget>();
        }
    }

    internal sealed class WriteEmbbededFileIntoTarget : INugetProjectSpecificCodeGen
    {
        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        FileInfo solutionFile,
                                        XDocument projectDocument,
                                        NugetProjectInfos minimalApiProjectInfos)
        {
            // Get all embedded files matching to the strcuture
            var embbededFiles = GetType().Assembly.GetManifestResourceNames();
            var webApiProjectResources = embbededFiles.Where(f => f.Contains("New.NugetProject.CodeGen.")).ToList();

            foreach (var webApiProjectResource in webApiProjectResources)
            {
                var fileExtension = Path.GetExtension(webApiProjectResource);
                var fileContent = EmbeddedFile.GetFileContentFrom(webApiProjectResource);

                var newFileContent = fileContent.Replace("$ProjectName$", minimalApiProjectInfos.NormalizedName)
                                                .Replace("$Namespace$", minimalApiProjectInfos.NormalizedName)
                                                .Replace("$RepoName$", minimalApiProjectInfos.RepoName);

                // Splitting at the double dot ".."
                var parts = webApiProjectResource.Split(["New.NugetProject.CodeGen."], StringSplitOptions.None);

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
                                                   .Replace(@$"{Path.DirectorySeparatorChar}Project{Path.DirectorySeparatorChar}Contracts{Path.DirectorySeparatorChar}", $@"{Path.DirectorySeparatorChar}{minimalApiProjectInfos.NormalizedName}.Contracts{Path.DirectorySeparatorChar}")
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
}
