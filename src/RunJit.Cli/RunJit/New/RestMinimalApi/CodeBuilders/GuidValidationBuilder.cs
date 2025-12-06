using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.New.RestMinimalApi;
using Solution.Parser.CSharp;

namespace RunJit.Cli.RunJit.New.RestMinimalApi.CodeBuilders
{
    internal static class AddGuidValidationBuilderExtension
    {
        internal static void AddGuidValidationBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IValidationBuilder, GuidValidationBuilder>();
        }
    }

    internal class GuidValidationBuilder : IValidationBuilder
    {
        public IEnumerable<string> BuildValidationFor(Property property,
                                                      string requestName,
                                                      CreateRestApiInfos createRestApiInfos)
        {
            var propertyName = property.Name;
            var propertyNameLower = property.Name.FirstCharToLower();

            if (property.Type.NotEqualsTo(nameof(Guid)))
            {
                yield break;
            }

            if (requestName.StartsWith("GetById") ||
                requestName.StartsWith("DeleteById"))
            {
                yield return """
                    if (request.$PropertyName$.IsEmpty())
                    {
                        var errorDetails = new ValidationErrorDetails()
                        {
                            CurrentValue = request.$PropertyName$,
                            Errors = [$"{nameof(request.$PropertyName$)} must not be empty."],
                            Samples = [Guid.NewGuid(), Guid.NewGuid()],
                        };

                        yield return (nameof(request.$PropertyName$), errorDetails);
                    }
                    """.Replace("$PropertyName$", propertyName)
                       .Replace("$PropertyNameLower$", propertyNameLower)
                       .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower);

                yield break;
            }

            if (requestName.StartsWith("GetById") ||
                requestName.StartsWith("DeleteById"))
            {
                // Query type validation
                // NULL is default -> so if i dont want to filter i dont set the query parameter so it is NULL per default
                yield return """
                    if (request.$PropertyName$.IsNull())
                    {
                        var errorDetails = new ValidationErrorDetails()
                        {
                            CurrentValue = request.$PropertyName$,
                            Errors = [$"{nameof(request.$PropertyName$)} must not be empty. We are not allowing to delete all data without filter. To filter results, provide a valid $PropertyNameLower$ (e.g., DELETE /$DomainNamePluralLower$?$PropertyNameLower$=Hello)."],
                            Samples = [Guid.NewGuid(), Guid.NewGuid()],
                        };

                        yield return (nameof(request.$PropertyName$), errorDetails);
                    }
                    """.Replace("$PropertyName$", propertyName)
                       .Replace("$PropertyNameLower$", propertyNameLower)
                       .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower);

                yield break;
            }

            yield return """
                if (request.$PropertyName$.IsEmpty())
                {
                    var errorDetails = new ValidationErrorDetails()
                    {
                        CurrentValue = request.$PropertyName$,
                        Errors = [$"{nameof(request.$PropertyName$)} must not be empty."],
                        Samples = [Guid.NewGuid(), Guid.NewGuid()],
                    };

                    yield return (nameof(request.$PropertyName$), errorDetails);
                }
                """.Replace("$PropertyName$", propertyName)
                   .Replace("$PropertyNameLower$", propertyNameLower)
                   .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower);
        }
    }
}
