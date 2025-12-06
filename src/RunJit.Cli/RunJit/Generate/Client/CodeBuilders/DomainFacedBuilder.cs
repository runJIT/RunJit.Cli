using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;

namespace RunJit.Cli.Generate.Client
{
    internal static class AddDomainFacedBuilderExtension
    {
        internal static void AddDomainFacedBuilder(this IServiceCollection services)
        {
            services.AddAssignExpressionBuilder();
            services.AddParamaterBuilder();
            services.AddServiceRegistrationBuilder();
            services.AddPropertiesBuilder();

            services.AddSingletonIfNotExists<DomainFacedBuilder>();
        }
    }

    // What we create here:
    // using System;
    // using Microsoft.Extensions.DependencyInjection;
    // using Api.Admin.V1;
    // 
    // namespace Api.Admin
    // {
    //     internal static class AddAdminFacadeExtension      <-- ServiceRegistrationBuilder
    //     {
    //         internal static void AddAdminFacade(this IServiceCollection services)
    //         {
    //             services.AddAdminV1();  
    //         }
    //     }
    // 
    //     public class AdminFacade <-- ServiceRegistrationBuilder
    //     {
    //         public AdminFacade(AdminV1 adminV1)  <-- ParameterBuilder
    //         {
    //             V1 = adminV1;   <-- AssignExpressionBuilder
    //         }
    // 
    // 
    //         public AdminV1 V1 { get; init; }     <-- PropertiesBuilder
    //     }
    // }
    internal sealed class DomainFacedBuilder(AssignExpressionBuilder assignExpressionBuilder,
                                             ParameterBuilder parameterBuilder,
                                             ServiceRegistrationBuilder serviceRegistrationBuilder,
                                             PropertiesBuilder propertiesBuilder)
    {
        private readonly string _facadeTemplate = EmbeddedFile.GetFileContentFrom("RunJit.Generate.Client.Templates.facade.rps");

        public ImmutableList<GeneratedFacade> BuildFrom(ImmutableList<GeneratedClientCodeForController> generatedClientCodeForEndpoints,
                                                        string projectName,
                                                        string clientName)
        {
            // UserV1, UserV2, ProjectsV1, ProjectsV2
            // Each domain one facade
            // UserFacade
            // => UserV1
            // => UserV2
            var groupedControllers = generatedClientCodeForEndpoints.GroupBy(g => g.ControllerInfo.GroupName).ToImmutableList();

            var facades = groupedControllers.Select(group => BuildFrom(group, projectName, clientName)).ToImmutableList();

            return facades;
        }

        private GeneratedFacade BuildFrom(IGrouping<string, GeneratedClientCodeForController> groupedEndpoints,
                                          string projectName,
                                          string clientName)
        {
            var domain = groupedEndpoints.Key;
            var neutralDomain = domain.Replace("Controller", string.Empty);
            var serviceRegistrations = serviceRegistrationBuilder.BuildFrom(groupedEndpoints);
            var paramerters = parameterBuilder.BuildFrom(groupedEndpoints);
            var assignmentExpressions = assignExpressionBuilder.BuildFrom(groupedEndpoints);
            var properties = propertiesBuilder.BuildFrom(groupedEndpoints);

            var @namespace = $"{projectName}.{ClientGenConstants.Api}.{neutralDomain}";

            var usings = groupedEndpoints.Select(endpoint =>
                                                 {
                                                     if (endpoint.ControllerInfo.Version.IsNull())
                                                     {
                                                         return $"using {@namespace};";
                                                     }

                                                     return $"using {@namespace}.{endpoint.ControllerInfo.Version.Normalized};";
                                                 }).Flatten(Environment.NewLine);

            var facadeName = $"{neutralDomain}Facade";

            var facadeClass = _facadeTemplate.Replace("$name$", facadeName)
                                             .Replace("$serviceRegistrations$", serviceRegistrations)
                                             .Replace("$paramters$", paramerters)
                                             .Replace("$assignmentExpressions$", assignmentExpressions)
                                             .Replace("$projectName$", projectName)
                                             .Replace("$clientName$", clientName)
                                             .Replace("$properties$", properties)
                                             .Replace("$namespace$", @namespace)
                                             .Replace("$usings$", usings);

            return new GeneratedFacade(groupedEndpoints.ToImmutableList(), facadeClass, neutralDomain,
                                       facadeName);
        }
    }
}
