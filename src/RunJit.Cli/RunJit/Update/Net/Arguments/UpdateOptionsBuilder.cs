using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Update
{
    internal static class AddDotNetArgumentsBuilderExtension
    {
        internal static void AddDotNetArgumentsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetArgumentsBuilder, DotNetArgumentsBuilder>();
        }
    }

    internal interface IDotNetArgumentsBuilder
    {
        IEnumerable<System.CommandLine.Argument> Build();
    }

    internal sealed class DotNetArgumentsBuilder : IDotNetArgumentsBuilder
    {
        public IEnumerable<System.CommandLine.Argument> Build()
        {
            yield break;

            // yield return BuildSourceOption();
        }

        //public System.CommandLine.Argument BuildSourceOption()
        //{
        //    return new Argument<string> { Name = "solutionFile" };
        //}
    }
}
