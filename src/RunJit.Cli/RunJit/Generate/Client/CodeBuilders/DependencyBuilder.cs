using System.Text;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;

namespace RunJit.Cli.Generate.Client
{
    internal static class AddDependencyBuilderExtension
    {
        internal static void AddDependencyBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<DependencyBuilder>();
        }
    }

    // <summary>
    // What we create here:
    // - Depending on the generated client we will detect all dependencies which are necessary to build up.
    // Sample:
    // - new AdminFacade(new AdminV1(httpClientHandler)), new AliveFacade(new AliveV1(httpClientHandler)),
    // </summary>
    internal sealed class DependencyBuilder
    {
        internal string BuildFrom(GeneratedClient generatedClient)
        {
            var stringBuilder = new StringBuilder();

            for (var index = 0; index < generatedClient.Facades.Count; index++)
            {
                // New statement -> new UserFacade(
                var facade = generatedClient.Facades[index];
                stringBuilder.Append($"new {facade.FacadeName}(");

                // Generate parameter for the facade -> new UserFacade(new UserV1(httpClientHandler), new UserV2(httpClientHandler)
                var versionDomains = facade.Endpoints.Select(endpoint =>
                                                             {
                                                                 if (endpoint.ControllerInfo.Version.IsNotNull())
                                                                 {
                                                                     return $"new {endpoint.Domain}(httpClientHandler)";
                                                                 }

                                                                 return $"httpClientHandler";
                                                             }).Flatten(", ");

                stringBuilder.Append(versionDomains);

                // If we reach the last parameter we have to close the new statement correctly
                // -> new UserFacade(new UserV1(httpClientHandler), new UserV2(httpClientHandler))
                stringBuilder.Append(index < generatedClient.Facades.Count - 1 ? "), " : ")");
            }

            var result = stringBuilder.ToString();

            return result;
        }
    }
}
