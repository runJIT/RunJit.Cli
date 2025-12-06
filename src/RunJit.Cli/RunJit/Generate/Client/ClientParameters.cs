namespace RunJit.Cli.RunJit.Generate.Client
{
    internal sealed record ClientParameters(bool UseVisualStudio,
                                            bool Build,
                                            FileInfo SolutionFile,
                                            bool UseOpenApiJson);
}
