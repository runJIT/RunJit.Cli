using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;

namespace RunJit.Cli.Generate.Client
{
    internal static class AddServiceRegistrationBuilderExtension
    {
        internal static void AddServiceRegistrationBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ServiceRegistrationBuilder>();
        }
    }

    // What we create here:
    // - We create here the resharper project settings to setup namespace providers correctly
    // 
    // Samples:
    // 
    // services.AddAdminFacade();
    // services.AddAliveFacade();
    // services.AddBulkResourcesFacade();
    // services.AddBulkResourceTranslationsFacade();
    // services.AddCachingFacade();
    // services.AddCategoryFacade();
    internal sealed class ServiceRegistrationBuilder
    {
        internal string BuildFrom(IImmutableList<GeneratedFacade> facades)
        {
            var parameters = facades.Select(f => $"\t\t\tservices.Add{f.FacadeName}(configuration);")
                                    .Flatten(Environment.NewLine);

            return parameters;
        }

        internal string BuildFrom(IGrouping<string, GeneratedClientCodeForController> groupedEndpoints)
        {
            var parameters = groupedEndpoints.Select(f => $"\t\t\tservices.Add{f.Domain}(configuration);")
                                             .Flatten(Environment.NewLine);

            return parameters;
        }
    }
}
