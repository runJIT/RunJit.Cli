using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.New.NugetProject
{
    internal static class AddEmbeddedFileSettingsExtension
    {
        internal static void AddEmbeddedFileSettings(this IServiceCollection services)
        {
            services.AddRetryHelper();

            services.AddSingletonIfNotExists<INugetProjectSpecificCodeGen, EmbeddedFileSettings>();
            services.AddSingletonIfNotExists<INugetProjectTestSpecificCodeGen, EmbeddedFileSettings>();
        }
    }

    internal sealed class EmbeddedFileSettings(ConsoleService consoleService) : INugetProjectSpecificCodeGen,
                                                                                INugetProjectTestSpecificCodeGen
    {
        public Task GenerateAsync(FileInfo projectFileInfo,
                                  FileInfo solutionFile,
                                  XDocument projectDocument,
                                  NugetProjectInfos minimalApiProjectInfos)
        {
            // 1. Create a new item group for embedded files
            //    <ItemGroup>
            //        <EmbeddedResource Include="**\*.json" Exclude="bin\**\*;obj\**\*" />
            //    </ItemGroup>
            var toolEmbeddedFileSettingsComment = new XComment("Embedded files area");

            // 2. Add wildcards for files which should be embedded
            var itemGroup = new XElement("ItemGroup");
            var embeddedFileElements = GetEmbeddedResources(".txt", ".json", ".sh").ToList();
            embeddedFileElements.ForEach(element => itemGroup.Add(element));

            // 3. Add the comment and new PropertyGroup to the root of the project file
            projectDocument.Root!.Add(toolEmbeddedFileSettingsComment, itemGroup);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully modified {projectFileInfo.FullName} with .Net tool specific settings");

            return Task.CompletedTask;
        }

        private IEnumerable<XElement> GetEmbeddedResources(params string[] fileExtensions)
        {
            foreach (var fileExtension in fileExtensions)
            {
                yield return BuildEmbeddedResourceElement(fileExtension);
            }
        }

        private XElement BuildEmbeddedResourceElement(string extension)
        {
            var embeddedResourceElement = new XElement("EmbeddedResource");
            var includeAttribute = new XAttribute("Include", @$"**\*.{extension.TrimStart('.')}");
            var excludeAttribute = new XAttribute("Exclude", @"bin\**\*;obj\**\*");
            embeddedResourceElement.Add(includeAttribute);
            embeddedResourceElement.Add(excludeAttribute);

            return embeddedResourceElement;
        }
    }
}
