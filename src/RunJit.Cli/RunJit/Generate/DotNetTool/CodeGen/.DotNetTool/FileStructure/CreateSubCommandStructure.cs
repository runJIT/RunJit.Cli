using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool.Models;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddCreateSubCommandStructureExtension
    {
        internal static void AddCreateSubCommandStructure(this IServiceCollection services)
        {
            services.AddCommandBuilderForSubCommands();
            services.AddSubCommandInterfaceBuilder();

            services.AddSingletonIfNotExists<IBuildCommandFileStructure, CreateSubCommandStructure>();
        }
    }

    internal sealed class CreateSubCommandStructure(CommandBuilderForSubCommands commandBuilderForSubCommands,
                                                    SubCommandInterfaceBuilder subCommandInterfaceBuilder) : IBuildCommandFileStructure
    {
        public void Create(string projectName,
                           CommandInfo? parentCommandInfo,
                           CommandTypeCollector commandTypeCollector,
                           string currentPath,
                           NameSpaceCollector namespaceCollector,
                           DirectoryInfo subCommnandDirectoryInfo,
                           CommandInfo commandInfo,
                           DotNetToolInfos dotNetToolName)
        {
            if (!commandInfo.SubCommands.IsNotNull() || commandInfo.SubCommands.IsEmpty())
            {
                return;
            }

            var result = commandBuilderForSubCommands.Build(projectName, commandInfo, parentCommandInfo,
                                                            currentPath);

            var filePath0 = new FileInfo(Path.Combine(subCommnandDirectoryInfo.FullName, $"{commandInfo.NormalizedName}CommandBuilder.cs"));

            File.WriteAllText(filePath0.FullName, result);

            var subCommandBuilder = subCommandInterfaceBuilder.Build(projectName, commandInfo, parentCommandInfo,
                                                                     currentPath);

            var filePath1 = new FileInfo(Path.Combine(subCommnandDirectoryInfo.FullName, $"I{commandInfo.NormalizedName}SubCommandBuilder.cs"));

            File.WriteAllText(filePath1.FullName, subCommandBuilder);

            var splittedNamespace = currentPath.Split(".").ToList();
            splittedNamespace.Remove(splittedNamespace.Last());
            var newNamespaceForInterface = splittedNamespace.Flatten(".");

            var implementation = $"{currentPath}.{filePath0.NameWithoutExtension()}";
            var interfaceType = parentCommandInfo.IsNull() ? implementation : $"{newNamespaceForInterface}.I{parentCommandInfo?.NormalizedName}SubCommandBuilder";

            commandTypeCollector.Add(commandInfo, new TypeToRegister(interfaceType, implementation));
        }
    }
}
