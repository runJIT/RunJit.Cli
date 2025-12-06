using System.Diagnostics;
using RunJit.Cli.Models;

namespace RunJit.Cli.RunJit.Generate.Client
{
    [DebuggerDisplay("Project: {" + nameof(ProjectName) + "} ToolName: {" + nameof(Models.DotNetToolName.Name) + "}")]
    public record Client(string ProjectName,
                         DotNetToolName DotNetToolName,
                         FileInfo SolutionFileInfo,
                         FileSystemInfo TargetDirectory,
                         bool UseOpenApiJson);
}
