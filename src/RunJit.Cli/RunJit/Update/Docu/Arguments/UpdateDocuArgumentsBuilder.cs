using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Update.Docu
{
    internal static class AddUpdateDocuArgumentsBuilderExtension
    {
        internal static void AddUpdateDocuArgumentsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UpdateDocuArgumentsBuilder>();
        }
    }

    internal sealed class UpdateDocuArgumentsBuilder
    {
        public IEnumerable<System.CommandLine.Argument> Build()
        {
            yield break;
        }
    }
}
