using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.New.RestMinimalApi
{
    internal static class AddWriteEmbbededFileIntoTargetExtension
    {
        internal static void AddWriteEmbbededFileIntoTarget(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRestMinimalApiSpecificCodeGen, WriteEmbbededFileIntoTarget>();
        }
    }

    internal sealed class WriteEmbbededFileIntoTarget : IRestMinimalApiSpecificCodeGen
    {
        public async Task GenerateAsync(FileInfo solutionFileInfo,
                                        FileInfo webApiProject,
                                        CreateRestApiInfos createRestApiInfos)
        {
            // Get all embedded files matching to the strcuture
            var embbededFiles = GetType().Assembly.GetManifestResourceNames();
            var webApiProjectResources = embbededFiles.Where(f => f.Contains("New.RestMinimalApi.CodeGen.")).ToList();

            foreach (var webApiProjectResource in webApiProjectResources)
            {
                var fileExtension = Path.GetExtension(webApiProjectResource);
                var fileContent = EmbeddedFile.GetFileContentFrom(webApiProjectResource);

                var newFileContent = fileContent.Replace("$ProjectName$", createRestApiInfos.ProjectName)
                                                .Replace("$Namespace$", createRestApiInfos.ProjectName)
                                                .Replace("$Version$", createRestApiInfos.Version.ToInvariantString())
                                                .Replace("$DomainModel$", createRestApiInfos.DomainModelCode)
                                                .Replace("$EntityModel$", createRestApiInfos.EntityModelCode)
                                                .Replace("$DomainName$", createRestApiInfos.DomainName)
                                                .Replace("$DomainNameLower$", createRestApiInfos.DomainNameLower)
                                                .Replace("$DomainNamePlural$", createRestApiInfos.DomainNamePlural)
                                                .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower)
                                                .Replace("$PropertyMappings$", createRestApiInfos.PropertyMappings)
                                                .Replace("$PropertiesWithoutId$", createRestApiInfos.PropertiesWithoutId)
                                                .Replace("$IdPropertyName$", createRestApiInfos.IdPropertyName)
                                                .Replace("$IdUrlName$", createRestApiInfos.IdPropertyName.FirstCharToLower());

                if (newFileContent.Contains("GetAllProjectsResponse"))
                {

                }
                
                // Splitting at the double dot ".."
                var parts = webApiProjectResource.Split(["New.RestMinimalApi.CodeGen."], StringSplitOptions.None);

                if (parts.Length == 2)
                {
                    // Replacing dots with backslashes in the file path part
                    // Important  from $ becomes _ in embedded resources
                    var part = parts[1];

                    part = part.Replace("Project", createRestApiInfos.ProjectName)
                               .Replace("_ProjectName_", createRestApiInfos.ProjectName)
                               .Replace("_Namespace_", createRestApiInfos.ProjectName)
                               .Replace("_Version_", createRestApiInfos.Version.ToInvariantString())
                               .Replace("_DomainModel_", createRestApiInfos.DomainModelCode)
                               .Replace("_DomainName_", createRestApiInfos.DomainName)
                               .Replace("_DomainNameLower_", createRestApiInfos.DomainNameLower)
                               .Replace("_DomainNamePlural_", createRestApiInfos.DomainNamePlural)
                               .Replace("_DomainNamePluralLower_", createRestApiInfos.DomainNamePluralLower)
                               .Replace("_PropertyMappings_", createRestApiInfos.PropertyMappings)
                               .Replace("$ProjectName$", createRestApiInfos.ProjectName)
                               .Replace("$Namespace$", createRestApiInfos.ProjectName)
                               .Replace("$Version$", createRestApiInfos.Version.ToInvariantString())
                               .Replace("$DomainModel$", createRestApiInfos.DomainModelCode)
                               .Replace("$DomainName$", createRestApiInfos.DomainName)
                               .Replace("$DomainNameLower$", createRestApiInfos.DomainNameLower)
                               .Replace("$DomainNamePlural$", createRestApiInfos.DomainNamePlural)
                               .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower)
                               .Replace("$PropertyMappings$", createRestApiInfos.PropertyMappings)
                               .Replace("$IdPropertyName$", createRestApiInfos.IdPropertyName)
                               .Replace("$IdUrlName$", createRestApiInfos.IdPropertyName.FirstCharToLower());

                    
                    var normalizedPart = part;

                    var removeFileExtensions = part.Replace(fileExtension, string.Empty);

                    var transformedPath = removeFileExtensions.TrimStart('.').Replace('.', Path.DirectorySeparatorChar);

                    // Concatenating the final path
                    // Special folders have to be carefully handled !
                    if (normalizedPart.StartsWith("."))
                    {
                        transformedPath = $".{transformedPath}";
                    }

                    var withFileExtension = $"{transformedPath.TrimEnd(Path.DirectorySeparatorChar)}{fileExtension}".Replace("..", ".");

                    var finalPath = Path.Combine(webApiProject.Directory!.FullName, withFileExtension.TrimStart(Path.DirectorySeparatorChar));

                    //var projectNamePath = finalPath.Replace(@$"{Path.DirectorySeparatorChar}Project{Path.DirectorySeparatorChar}Test{Path.DirectorySeparatorChar}", $@"{Path.DirectorySeparatorChar}{createRestApiInfos.ProjectName}.Test{Path.DirectorySeparatorChar}")
                    //                               .Replace(@$"{Path.DirectorySeparatorChar}Project{Path.DirectorySeparatorChar}", $@"{Path.DirectorySeparatorChar}{createRestApiInfos.ProjectName}{Path.DirectorySeparatorChar}");

                    var fileInfo = new FileInfo(finalPath);

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
