using System.CommandLine;

namespace RunJit.Cli.Migrate.Endpoints
{
    internal interface IEndpointsSubCommandBuilder
    {
        Command Build();
    }
}
