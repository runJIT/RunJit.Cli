using Solution.Parser.CSharp;

namespace RunJit.Cli.Services.Endpoints
{
    public record RequestType(DeclarationBase Declaration,
                              string Type);
}
