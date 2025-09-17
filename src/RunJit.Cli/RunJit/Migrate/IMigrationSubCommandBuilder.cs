using System.CommandLine;

namespace RunJit.Cli.Migrate
{
    internal interface IMigrationSubCommandBuilder
    {
        Command Build();
    }
}
