using System.Collections.Immutable;
using System.Reflection;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Generate.Client;
using RunJit.Cli.RunJit.Generate.Client;
using RunJit.Cli.Services.Endpoints;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Services
{
    internal static class AddMethodParserExtension
    {
        internal static void AddMethodParser(this IServiceCollection services)
        {
            services.AddDataTypeFinder();
            services.AddUrlBuilder();
            services.AddModelNormalizer();
            services.AddParameterNormalizer();
            services.AddResponseTypeNormalizer();

            services.AddSingletonIfNotExists<MethodParser>();
        }
    }

    internal sealed class MethodParser(DataTypeFinder dataTypeFinder,
                                       UrlBuilder urlBuilder,
                                       ModelNormalizer modelNormalizer,
                                       ParameterNormalizer parameterNormalizer,
                                       ResponseTypeNormalizer responseTypeNormalizer)
    {
        internal ImmutableList<MethodInfos> Parse(ImmutableList<Method> methods,
                                                  string baseUrl,
                                                  ImmutableList<MethodInfo> reflectionTypes,
                                                  ImmutableList<CSharpSyntaxTree> syntaxTrees)
        {
            // Only methods which represents a http endpoint
            var publicMethods = methods.Where(m => m.Attributes.Any(attribute => attribute.Name.StartWith("Http"))).ToImmutableList();

            return publicMethods.Select(method => Parse(method, baseUrl, reflectionTypes,
                                                        syntaxTrees)).ToImmutableList();
        }

        private MethodInfos Parse(Method method,
                                  string baseUrl,
                                  ImmutableList<MethodInfo> reflectionTypes,
                                  ImmutableList<CSharpSyntaxTree> syntaxTrees)
        {
            ImmutableList<MethodInfo> methods = reflectionTypes.Where(m => m.Name.EqualsTo(method.Name) &&
                                                                           m.GetParameters().Length.EqualsTo(method.Parameters.Count)).ToImmutableList();

            // Simple workaround fallback -> this is a bug in the code method name and parameter same !
            if (methods.Count > 1)
            {
                methods = FilterByReturnType(methods, method);
            }

            if (methods.Count != 1)
            {
                throw new RunJitException($"Unexpected method count for method name: {method.Name}. Please go sure your current code state match the assemblies in your project. Please try to rebuild the solution before you run the client gen again");
            }

            var methodReflection = methods[0];
            var httpAttribute = method.Attributes.FirstOrDefault(a => a.Name.StartWith("Http"));
            var httpAction = httpAttribute?.Name.Replace("Http", string.Empty) ?? string.Empty;
            var swaggerOperation = method.Attributes.FirstOrDefault(a => a.Name.EqualsTo("SwaggerOperation"));
            var swaggerOperationId = swaggerOperation?.Arguments.FirstOrDefault(a => a.Contains("OperationId"))?.Split("=").Last().Replace(@"""", string.Empty)?.Trim() ?? string.Empty;
            var produceResponseType = GetProduceResponseType(method);
            var relativeUrl = urlBuilder.BuildFrom(baseUrl, method);
            var reflectionParameters = methodReflection.GetParameters().ToImmutableList();
            var allModels = dataTypeFinder.FindDataType(methodReflection, syntaxTrees).ToImmutableList();
            var parameters = parameterNormalizer.Normalize(method, reflectionParameters, allModels);
            var normalizeDataTypes = modelNormalizer.Normalize(allModels).ToImmutableList();
            var responseType = responseTypeNormalizer.GetResponseType(methodReflection, method, normalizeDataTypes);

            return new MethodInfos
                   {
                       Name = method.Name,
                       SwaggerOperationId = swaggerOperationId,
                       HttpAction = httpAction,
                       ProduceResponseTypes = produceResponseType,
                       RelativeUrl = relativeUrl,
                       Parameters = parameters,
                       ResponseType = responseType,
                       RequestType = null, // ToDo not sure how realy to detect ot many possibilities here,
                       Attributes = method.Attributes,
                       Models = normalizeDataTypes
                   };
        }

        private static ImmutableList<ProduceResponseTypes> GetProduceResponseType(Method method)
        {
            var produceResponseType = method.Attributes.Where(attribute => attribute.Name.EqualsTo("ProducesResponseType"))
                                            .Select(produce =>
                                                    {
                                                        var type = produce.Arguments.FirstOrDefault() ?? string.Empty;
                                                        var code = produce.Arguments.LastOrDefault()?.ToIntOrDefault() ?? 0;

                                                        return new ProduceResponseTypes(type, code);
                                                    }).ToImmutableList();

            return produceResponseType;
        }

        private ImmutableList<MethodInfo> FilterByReturnType(ImmutableList<MethodInfo> methods,
                                                             Method method)
        {
            var filteredMethods = methods.Where(m =>
                                                {
                                                    var returnTypes = m.ReturnType.GetAllGenericArguments();

                                                    return returnTypes.Any(t => method.ReturnParameter.Contains(t.Name));
                                                }).ToImmutableList();

            return filteredMethods;
        }
    }
}
