using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Update.GlobalJson
{
    internal static class AddUpdateGlobalJsonArgumentsBuilderExtension
    {
        internal static void AddUpdateGlobalJsonArgumentsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UpdateGlobalJsonArgumentsBuilder>();
        }
    }

    internal sealed class UpdateGlobalJsonArgumentsBuilder
    {
        public IEnumerable<System.CommandLine.Argument> Build()
        {
            yield break;
        }
    }
}
