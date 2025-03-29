using System.Diagnostics;

namespace RunJit.Cli.New.NugetProject
{
    [DebuggerDisplay("Project: {" + nameof(ProjectName) + "} ToolName: {" + nameof(NormalizedName) + "}")]
    internal sealed record NugetProjectInfos
    {
        public required string ProjectName { get; init; }
        
        public required string ContractProjectName { get; init; }

        public required string Name { get; init; }

        public required string NormalizedName { get; init; }
        
        public required string RepoName { get; init; }

        public required string NetVersion { get; init; } = "net9.0";
    }
}
