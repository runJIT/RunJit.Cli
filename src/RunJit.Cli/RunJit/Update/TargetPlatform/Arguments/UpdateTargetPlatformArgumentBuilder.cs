using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Update.TargetPlatform
{
    internal static class AddUpdateTargetPlatformArgumentBuilderExtension
    {
        internal static void AddUpdateTargetPlatformArgumentBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IUpdateTargetPlatformArgumentBuilder, UpdateTargetPlatformArgumentBuilder>();
        }
    }

    internal interface IUpdateTargetPlatformArgumentBuilder
    {
        IEnumerable<System.CommandLine.Argument> Build();
    }

    internal sealed class UpdateTargetPlatformArgumentBuilder : IUpdateTargetPlatformArgumentBuilder
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
