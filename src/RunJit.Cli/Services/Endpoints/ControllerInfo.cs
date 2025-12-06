using System.Collections.Immutable;
using System.Diagnostics;
using Attribute = Solution.Parser.CSharp.Attribute;

namespace RunJit.Cli.Services.Endpoints
{
    [DebuggerDisplay("{Name}")]
    public record ControllerInfo
    {
        public required string Name { get; init; } = string.Empty;

        public required string DomainName { get; init; } = string.Empty;

        public required VersionInfo Version { get; init; } = new("1.0", "V1");

        public required string BaseUrl { get; init; } = string.Empty;

        public required string GroupName { get; init; } = string.Empty;

        public ImmutableList<MethodInfos> Methods { get; init; } = ImmutableList<MethodInfos>.Empty;

        public ImmutableList<Attribute> Attributes { get; set; } = ImmutableList<Attribute>.Empty;
    }
}
