using System.Collections.Immutable;
using System.Diagnostics;
using Solution.Parser.CSharp;
using Attribute = Solution.Parser.CSharp.Attribute;

namespace RunJit.Cli.Services.Endpoints
{
    [DebuggerDisplay("{Name}")]
    public record MethodInfos
    {
        public required string Name { get; init; } = string.Empty;

        public required string SwaggerOperationId { get; init; } = string.Empty;

        public required string HttpAction { get; init; } = string.Empty;

        public required string RelativeUrl { get; init; } = string.Empty;

        public ImmutableList<Parameter> Parameters { get; init; } = ImmutableList<Parameter>.Empty;

        public required RequestType? RequestType { get; init; }

        public required ResponseType ResponseType { get; init; }

        public ImmutableList<ProduceResponseTypes> ProduceResponseTypes { get; init; } = ImmutableList<ProduceResponseTypes>.Empty;

        public ImmutableList<Attribute> Attributes { get; init; } = ImmutableList<Attribute>.Empty;

        public ImmutableList<DeclarationBase> Models { get; init; } = ImmutableList<DeclarationBase>.Empty;
    }
}
