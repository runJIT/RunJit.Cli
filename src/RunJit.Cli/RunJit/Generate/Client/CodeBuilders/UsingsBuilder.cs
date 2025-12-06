using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;

namespace RunJit.Cli.Generate.Client
{
    internal static class AddUsingsBuilderExtension
    {
        internal static void AddUsingsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UsingsBuilder>();
        }
    }

    // What we create here:
    // - We create needed using needed from the facades
    // 
    // Samples:
    //
    // using Api.Admin;
    // using Api.Alive;
    // using Api.BulkResources;
    // using Api.BulkResourceTranslations;
    // using Api.Caching;
    // using Api.Category;
    // using Api.Countries;
    // using Api.ErrorLog;
    // using Api.Files;
    // using Api.Filter;
    internal sealed class UsingsBuilder
    {
        internal string BuildFrom(ImmutableList<GeneratedFacade> facades,
                                  string projectName)
        {
            var usings = facades.Select(facade => $"using {projectName}.{ClientGenConstants.Api}.{facade.Domain};").Flatten(Environment.NewLine);

            return usings;
        }

        internal string BuildFrom(GeneratedClient generatedClient,
                                  string projectName)
        {
            var usings = CollectUsings(generatedClient).Distinct().Flatten(Environment.NewLine);

            return usings;

            IEnumerable<string> CollectUsings(GeneratedClient generatedClient)
            {
                foreach (var facade in generatedClient.Facades)
                {
                    // Users
                    yield return $"using {projectName}.{ClientGenConstants.Api}.{facade.Domain};";

                    foreach (var endpoint in facade.Endpoints)
                    {
                        // Users.V1
                        if (endpoint.ControllerInfo.Version.IsNotNull())
                        {
                            yield return $"using {projectName}.{ClientGenConstants.Api}.{facade.Domain}.{endpoint.ControllerInfo.Version.Normalized};";
                        }
                        else
                        {
                            yield return $"using {projectName}.{ClientGenConstants.Api}.{facade.Domain};";
                        }
                    }
                }
            }
        }
    }
}
