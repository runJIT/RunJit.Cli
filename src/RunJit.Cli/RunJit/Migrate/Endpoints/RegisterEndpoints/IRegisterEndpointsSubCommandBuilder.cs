using System.CommandLine;

namespace RunJit.Cli.Migrate.Endpoints.RegisterEndpoints
{
    internal interface IRegisterEndpointsSubCommandBuilder
    {
        Command Build();
    }
}
