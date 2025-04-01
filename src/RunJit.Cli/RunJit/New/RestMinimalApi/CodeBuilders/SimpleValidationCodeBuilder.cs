using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.New.RestMinimalApi;
using Solution.Parser.CSharp;

namespace RunJit.Cli.RunJit.New.RestMinimalApi.CodeBuilders
{
    internal static class AddSimpleValidationCodeBuilderExtension
    {
        internal static void AddSimpleValidationCodeBuilder(this IServiceCollection services)
        {
            services.AddStringValidationBuilder();
            services.AddGuidValidationBuilder();
            
            services.AddSingletonIfNotExists<SimpleValidationCodeBuilder>();
        }
    }

    internal class SimpleValidationCodeBuilder(IEnumerable<IValidationBuilder> validationBuilders)
    {
        internal string BuildSimpleValidations(IEnumerable<Property> properties,
                                               string requestName,
                                               CreateRestApiInfos createRestApiInfos)
        {
            // Yes currently we only create for flatten types
            var allValidations = properties.SelectMany(p => validationBuilders.SelectMany(v => v.BuildValidationFor(p, requestName, createRestApiInfos))).Distinct().ToList();

            // all validations
            var flatten = allValidations.Flatten($"{Environment.NewLine}");
            return flatten;
        }
    }

    internal interface IValidationBuilder
    {
        internal IEnumerable<string> BuildValidationFor(Property property,
                                                        string requestName,
                                                        CreateRestApiInfos createRestApiInfos);
    }
}
