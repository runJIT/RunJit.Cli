using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Migrate.Endpoints.RegisterEndpoints.Options
{
    internal static class AddRegisterEndpointsOptionsBuilderExtension
    {
        internal static void AddRegisterEndpointsOptionsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRegisterEndpointsOptionsBuilder, RegisterEndpointsOptionsBuilder>();
        }
    }

    internal interface IRegisterEndpointsOptionsBuilder
    {
        IEnumerable<Option> Build();
    }

    internal sealed class RegisterEndpointsOptionsBuilder : IRegisterEndpointsOptionsBuilder
    {
        public IEnumerable<Option> Build()
        {
            yield return SolutionFile();
            yield return WebApiProjectPath();
        }

        private static Option SolutionFile()
        {
            return new Option(new[] { "--solution", "-s" }, "The solution file path.")
                   {
                       Required = true,
                       Argument = new Argument<string>("solution") { Description = "The .sln file path" }
                   };
        }

        private static Option WebApiProjectPath()
        {
            return new Option(new[] { "--webapi-project", "-wp" }, "The Web API csproj path.")
                   {
                       Required = true,
                       Argument = new Argument<string>("webApiProject") { Description = "Path to the Web API project (.csproj)" }
                   };
        }
    }
}
