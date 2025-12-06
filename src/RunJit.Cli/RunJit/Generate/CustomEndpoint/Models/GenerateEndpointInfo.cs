using System.Collections.Immutable;

namespace RunJit.Cli.RunJit.Generate.CustomEndpoint
{
    public record EndpointData
    {
        public ImmutableList<Template> Templates { get; init; } = ImmutableList<Template>.Empty;
    }

    public record EndpointAction
    {
        public required string HttpAction { get; init; }

        public required string Url { get; set; }

        public string DomainActionName { get; init; } = string.Empty;

        public ImmutableList<string> QueryParameters { get; init; } = ImmutableList<string>.Empty;

        public ImmutableList<string> AllowedResourceTypes { get; init; } = ImmutableList<string>.Empty;
    }

    public record Template
    {
        public required string Folder { get; init; }

        public ImmutableList<CodeFile> Files { get; init; } = ImmutableList<CodeFile>.Empty;

        public ImmutableList<Template> Templates { get; init; } = ImmutableList<Template>.Empty;
    }

    public record CodeFile
    {
        public required string Name { get; set; }

        public required string Content { get; set; }
    }

    internal sealed record UrlParameter(string Name,
                                        string Type);

    internal sealed record QueryParameter(string Name,
                                          string Type);
}
