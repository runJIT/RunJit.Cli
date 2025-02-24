using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Fix.ProjectSettings
{
    internal static class AddFixProjectSettingsArgumentsBuilderExtension
    {
        internal static void AddFixProjectSettingsArgumentsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IFixProjectSettingsArgumentsBuilder, FixProjectSettingsArgumentsBuilder>();
        }
    }

    internal interface IFixProjectSettingsArgumentsBuilder
    {
        IEnumerable<System.CommandLine.Argument> Build();
    }

    internal sealed class FixProjectSettingsArgumentsBuilder : IFixProjectSettingsArgumentsBuilder
    {
        public IEnumerable<System.CommandLine.Argument> Build()
        {
            yield return BuildSourceOption();
        }

        public System.CommandLine.Argument BuildSourceOption()
        {
            return new Argument<string> { Name = "solutionFile" };
        }
    }
}
