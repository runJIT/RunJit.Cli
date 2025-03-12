using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Generate.DotNetTool
{
    //internal static class AddProjectTypeCodeGenExtension
    //{
    //    internal static void AddProjectTypeCodeGen(this IServiceCollection services)
    //    {
    //        services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, ProjectTypeCodeGen>();
    //    }
    //}

    internal sealed class ProjectTypeCodeGen(ConsoleService consoleService) : IDotNetToolSpecificCodeGen
    {
        public Task GenerateAsync(FileInfo projectFileInfo,
                                  XDocument projectDocument,
                                  DotNetToolInfos dotNetToolInfos)
        {
            // 1. We have to go sure that the project file is a web project
            //    cause we are using http stuff and more. And all nugets from
            //    microsoft was set to deprecated so we have to use the correct
            //    way over the project type instead of using the nuget package
            //   <Project Sdk="Microsoft.NET.Sdk.Web">
            projectDocument.Root!.Attribute("Sdk")!.Value = "Microsoft.NET.Sdk.Web";

            // 2. Save the changes back to the .csproj file
            projectDocument.Save(projectFileInfo.FullName);

            // 3. Print success message
            consoleService.WriteSuccess($"Successfully modified {projectFileInfo.FullName} with .Net tool specific settings");

            return Task.CompletedTask;
        }
    }
}
