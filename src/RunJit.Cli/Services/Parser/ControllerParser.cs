using System.Collections.Immutable;
using System.Reflection;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;
using RunJit.Cli.Services.Endpoints;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Services
{
    internal static class AddControllerParserExtension
    {
        internal static void AddControllerParser(this IServiceCollection services)
        {
            services.AddMethodParser();

            services.AddSingletonIfNotExists<ControllerParser>();
        }
    }

    internal sealed class ControllerParser(MethodParser methodParser)
    {
        public ImmutableList<ControllerInfo> ExtractFrom(ImmutableList<CSharpSyntaxTree> syntaxTrees,
                                                         ImmutableList<Type> reflectionTypes)
        {
            // 1. Detect all controllers. Derived class like Controller, ControllerBase, ODataController and so on
            var controllers = syntaxTrees.GetAllControllers();

            // 2. Extract all needed infos for generation out
            var controllerInfos = Parse(controllers, reflectionTypes, syntaxTrees).ToImmutableList();

            return controllerInfos;
        }

        private IEnumerable<ControllerInfo> Parse(ImmutableList<Class> controllers,
                                                  ImmutableList<Type> reflectionTypes,
                                                  ImmutableList<CSharpSyntaxTree> syntaxTrees)
        {
            foreach (var controller in controllers)
            {
                // 0. Reflection controller
                var controllerType = reflectionTypes.FirstOrDefault(type => type.FullName.EqualsTo(controller.FullQualifiedName));

                if (controllerType.IsNull())
                {
                    continue;
                }

                // 1. Extract meta infos version, base url and son on.
                var version = controller.Attributes.FirstOrDefault(a => a.Name.EqualsTo("ApiVersion"))?.Arguments?.FirstOrDefault()?.Trim('"') ?? "1.0";

                var normalizedVersion = $"V{version.Replace(".0", string.Empty) // V1.0 => V1
                                                   .Replace(".", "_")}"; // V1.1 => V1_1}"

                var baseUrl = controller.Attributes.FirstOrDefault(a => a.Name.EqualsTo("Route"))?.Arguments.FirstOrDefault()?.Replace("{version:apiVersion}", version.ToLowerInvariant()).Trim('"') ?? string.Empty;

                // 2. Now parse all methods
                var methodReflection = controllerType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).ToImmutableList();

                var methods = methodParser.Parse(controller.Methods, baseUrl, methodReflection,
                                                 syntaxTrees);

                // 3. Use ApiExplorerSettings to override the base url
                var domainName = controller.Name.Replace("Controller", string.Empty);
                var groupName = controllerType.GetCustomAttribute<ApiExplorerSettingsAttribute>()?.GroupName ?? domainName;

                // 3. Return controller infos
                yield return new ControllerInfo
                             {
                                 Name = controller.Name,
                                 DomainName = domainName,
                                 Version = new VersionInfo(version, normalizedVersion),
                                 BaseUrl = baseUrl,
                                 Methods = methods,
                                 Attributes = controller.Attributes,
                                 GroupName = groupName
                             };
            }
        }
    }
}
