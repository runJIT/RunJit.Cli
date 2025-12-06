using System.Collections.Immutable;
using System.Diagnostics;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Services.Endpoints
{
    [DebuggerDisplay("{Name}")]
    public record EndpointInfo
    {
        public required string Name { get; init; } = string.Empty;

        public required string DomainName { get; init; } = string.Empty;

        public required VersionInfo? Version { get; init; } = new("1.0", "V1");

        public required string BaseUrl { get; init; } = string.Empty;

        public required string GroupName { get; init; } = string.Empty;

        public required string SwaggerOperationId { get; init; } = string.Empty;

        public required string HttpAction { get; init; } = string.Empty;

        public required string RelativeUrl { get; init; } = string.Empty;

        public ImmutableList<Parameter> Parameters { get; init; } = ImmutableList<Parameter>.Empty;

        public required RequestType? RequestType { get; init; }

        public required ResponseType ResponseType { get; init; }

        public ImmutableList<ProduceResponseTypes> ProduceResponseTypes { get; init; } = ImmutableList<ProduceResponseTypes>.Empty;

        public ImmutableList<DeclarationBase> Models { get; init; } = ImmutableList<DeclarationBase>.Empty;

        public ObsoleteInfo? ObsoleteInfo { get; init; }
    }

    public record ObsoleteInfo(string Info);

    [DebuggerDisplay("{GroupName}")]
    public record EndpointGroup
    {
        public ImmutableList<EndpointInfo> Endpoints { get; init; } = ImmutableList<EndpointInfo>.Empty;

        public required string GroupName { get; init; } = string.Empty;

        public required VersionInfo? Version { get; init; }

        public ObsoleteInfo? ObsoleteInfo { get; init; }
    }
}
