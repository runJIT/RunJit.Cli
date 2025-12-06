using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using FileInfo = System.IO.FileInfo;

namespace RunJit.Cli.RunJit.Generate.Client
{
    internal static class AddClientOptionsBuilderExtension
    {
        internal static void AddClientOptionsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IClientGenOptionsBuilder, ClientGenOptionsBuilder>();
        }
    }

    internal interface IClientGenOptionsBuilder
    {
        IEnumerable<Option> Build();
    }

    internal sealed class ClientGenOptionsBuilder : IClientGenOptionsBuilder
    {
        public IEnumerable<Option> Build()
        {
            yield return BuildUseVisualStudioOption();
            yield return Solution();
            yield return BuildMsBuild();
            yield return BuildUseOpenApiJson();
        }

        private Option BuildUseVisualStudioOption()
        {
            return new Option(new[] { "--use-visualstudio", "-uv" }, "Opens visual studio after generating the new dotnet tool. Only for Windows user !") { Required = false };
        }

        private Option BuildMsBuild()
        {
            return new Option(new[] { "--build", "-b" }, "Builds the target solution before creating the client") { Required = false };
        }

        private Option BuildUseOpenApiJson()
        {
            return new Option(new[] { "--use-open-api-json", "-uoaj" }, "Generate client via open api json") { Required = false };
        }

        private Option Solution()
        {
            return new Option(new[] { "--solution", "-s" }, @"File path to your backend solution where your api is implemented. Sample: D:\Projects\ClientGen\ClientGen.sln")
                   {
                       Required = false,
                       Argument = new Argument<FileInfo>("solution") { Description = @"File path to your backend solution where your api is implemented. Sample: D:\Projects\ClientGen\ClientGen.sln" }
                   };
        }
    }
}
