using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using Solution.Parser.Project;

namespace RunJit.Cli.Generate.DotNetTool.DotNetTool.Test
{
    internal static class AddProjectSettingsCodeGenExtension
    {
        internal static void AddProjectSettingsCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolTestSpecificCodeGen, ProjectSettingsCodeGen>();
        }
    }

    internal sealed class ProjectSettingsCodeGen(ConsoleService consoleService) : IDotNetToolTestSpecificCodeGen
    {
        public Task GenerateAsync(FileInfo projectFileInfo,
                                  XDocument projectDocument,
                                  DotNetToolInfos dotNetToolInfos,
                                  ProjectFile? webApiProject)
        {
            // 1. Create a new PropertyGroup for .NET tool settings
            //    <PropertyGroup>
            //        <IsPackable>false</IsPackable>
            //        <ImplicitUsings>enable</ImplicitUsings>
            //    </PropertyGroup>
            // 1. Create a new item group for embedded files
            //    <ItemGroup>
            //        <EmbeddedResource Include="**\*.json" Exclude="bin\**\*;obj\**\*" />
            //    </ItemGroup>
            var toolEmbeddedFileSettingsComment = new XComment("Test project specific area");

            // 2. Add wildcards for files which should be embedded
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
            projectDocument.Root!.Add(toolEmbeddedFileSettingsComment, propertyGroup);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully modified {projectFileInfo.FullName} with .Net tool specific settings");

            return Task.CompletedTask;
        }
    }
}
