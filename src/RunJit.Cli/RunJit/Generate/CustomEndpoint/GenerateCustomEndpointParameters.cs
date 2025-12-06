namespace RunJit.Cli.RunJit.Generate.CustomEndpoint
{
    internal sealed record GenerateCustomEndpointParameters(DirectoryInfo TargetFolder,
                                                            string EndpointData,
                                                            bool OverwriteCode);
}
