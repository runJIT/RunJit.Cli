using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;

namespace RunJit.Cli.Generate.Client
{
    internal static class AddAssignExpressionBuilderExtension
    {
        internal static void AddAssignExpressionBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<AssignExpressionBuilder>();
        }
    }

    // What we are build here:
    // - The assignment expression for properties or fields
    // Sample:
    // 
    // Admin = adminFacade;
    // Alive = aliveFacade;
    // BulkResources = bulkResourcesFacade;
    internal sealed class AssignExpressionBuilder
    {
        internal string BuildFrom(ImmutableList<GeneratedFacade> facades)
        {
            var assignments = facades.Select(f => $"\t\t\t{f.Domain}".FirstCharToUpper() + " = " + $"{f.FacadeName};".FirstCharToLower())
                                     .Flatten(Environment.NewLine);

            return assignments;
        }

        internal string BuildFrom(IGrouping<string, GeneratedClientCodeForController> groupedEndpoints)
        {
            var assignments = groupedEndpoints.Select(f =>
                                                      {
                                                          if (f.ControllerInfo.Version.IsNotNull())
                                                          {
                                                              return $"\t\t\t{f.ControllerInfo.Version.Normalized}".ToUpperInvariant() + " = " + $"{f.Domain};".FirstCharToLower();
                                                          }

                                                          return $"\t\t\t{f.Domain}".FirstCharToUpper() + " = " + $"{f.Domain};".FirstCharToLower();
                                                      })
                                              .Flatten(Environment.NewLine);

            return assignments;
        }
    }
}
