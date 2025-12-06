namespace RunJit.Cli.Generate.DotNetTool
{
    internal sealed record DotNetToolParameters(bool UseVisualStudio,
                                                bool Build,
                                                FileInfo SolutionFile,
                                                string ToolName);
}
