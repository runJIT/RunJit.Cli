using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using Solution.Parser.Solution;

namespace RunJit.Cli.New.MinimalApiProject
{
    internal static class AddEmbeddedFileSettingsExtension
    {
        internal static void AddEmbeddedFileSettings(this IServiceCollection services)
        {
            services.AddRetryHelper();

            services.AddSingletonIfNotExists<IMinimalApiProjectSpecificCodeGen, EmbeddedFileSettings>();
            services.AddSingletonIfNotExists<IMinimalApiProjectTestSpecificCodeGen, EmbeddedFileSettings>();
        }
    }

    internal sealed class EmbeddedFileSettings(ConsoleService consoleService) : IMinimalApiProjectSpecificCodeGen,
                                                                                IMinimalApiProjectTestSpecificCodeGen
    {
        public Task GenerateAsync(FileInfo projectFileInfo,
                                  FileInfo solutionFile,
                                  XDocument projectDocument,
                                  MinimalApiProjectInfos minimalApiProjectInfos)
        {
            // 1. Create a new item group for embedded files
            //    <ItemGroup>
            //        <EmbeddedResource Include="**\*.json" Exclude="bin\**\*;obj\**\*" />
            //    </ItemGroup>
            var toolEmbeddedFileSettingsComment = new XComment("Embedded files area");

            // 2. Add wildcards for files which should be embedded
            var itemGroup = new XElement("ItemGroup");
            var jsonEmbeddedResource = new XElement("EmbeddedResource");
            var jsonIncludeAttribute = new XAttribute("Include", @"**\*.json");
            var jsonExcludeAttribute = new XAttribute("Exclude", @"bin\**\*;obj\**\*");
            jsonEmbeddedResource.Add(jsonIncludeAttribute);
            jsonEmbeddedResource.Add(jsonExcludeAttribute);
            
            var txtEmbeddedResource = new XElement("EmbeddedResource");
            var txtIncludeAttribute = new XAttribute("Include", @"**\*.txt");
            var txtExcludeAttribute = new XAttribute("Exclude", @"bin\**\*;obj\**\*");
            txtEmbeddedResource.Add(txtIncludeAttribute);
            txtEmbeddedResource.Add(txtExcludeAttribute);
      
            
            itemGroup.Add(jsonEmbeddedResource);
            itemGroup.Add(txtEmbeddedResource);

            // 3. Add the comment and new PropertyGroup to the root of the project file
            projectDocument.Root!.Add(toolEmbeddedFileSettingsComment, itemGroup);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully modified {projectFileInfo.FullName} with .Net tool specific settings");

            return Task.CompletedTask;
        }
    }
}
