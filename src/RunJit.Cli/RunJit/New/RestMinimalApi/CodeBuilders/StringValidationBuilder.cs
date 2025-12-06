using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.New.RestMinimalApi;
using Solution.Parser.CSharp;

namespace RunJit.Cli.RunJit.New.RestMinimalApi.CodeBuilders
{
    internal static class AddStringValidationBuilderExtension
    {
        internal static void AddStringValidationBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IValidationBuilder, StringValidationBuilder>();
        }
    }

    internal class StringValidationBuilder : IValidationBuilder
    {
        public IEnumerable<string> BuildValidationFor(Property property,
                                                      string requestName,
                                                      CreateRestApiInfos createRestApiInfos)
        {
            var propertyName = property.Name;
            var propertyNameLower = property.Name.FirstCharToLower();

            if (property.Type.NotEqualsTo("string"))
            {
                yield break;
            }

            if (requestName.StartsWith("GetById") ||
                requestName.StartsWith("DeleteById"))
            {
                yield break;
            }

            if (requestName.StartsWith("Delete"))

            {
                yield return """
                    if (request.$PropertyName$.IsNull())
                    {
                        var errorDetails = new ValidationErrorDetails()
                        {
                            CurrentValue = request.$PropertyName$,
                            Errors = [$"{nameof(request.$PropertyName$)} must not be NULL. It is not allowed to delete all data with one call without filter. Provide a valid $PropertyNameLower$ (e.g., DELETE /$DomainNamePluralLower$?$PropertyNameLower$=Hello)."],
                            Samples = ["Hello world"],
                        };

                        yield return new PropertyValidationResult(nameof(request.$PropertyName$), errorDetails);
                        yield break;
                    }
                    """.Replace("$PropertyName$", propertyName)
                       .Replace("$PropertyNameLower$", propertyNameLower)
                       .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower);

                ;
            }

            // Simple check for non payload request
            if (requestName.StartsWith("Get") ||
                requestName.StartsWith("Delete"))
            {
                // Query type validation
                // NULL is default -> so if i dont want to filter i dont set the query parameter so it is NULL per default
                //yield return """
                //             if (request.$PropertyName$.IsNull())
                //             {
                //                 var errorDetails = new ValidationErrorDetails()
                //                 {
                //                     CurrentValue = request.$PropertyName$,
                //                     Errors = [$"{nameof(request.$PropertyName$)} must not be null. We are not allowing to delete all data without filter. To filter results, provide a valid $PropertyNameLower$ (e.g., DELETE /$DomainNamePluralLower$?$PropertyNameLower$=Hello)."],
                //                     Samples = ["Hello world"],
                //                 };

                //                 yield return (nameof(request.$PropertyName$), errorDetails);
                //             }
                //             """.Replace("$PropertyName$", propertyName)
                //                .Replace("$PropertyNameLower$", propertyNameLower)
                //                .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower);

                yield return """
                             if (request.$PropertyName$.IsNull())
                             {
                                 yield break;
                             }
                             """;

                yield return """
                    if (request.$PropertyName$.IsWhitespace())
                    {
                        var errorDetails = new ValidationErrorDetails()
                        {
                            CurrentValue = request.$PropertyName$,
                            Errors = [$"{nameof(request.$PropertyName$)} must not be one or only whitespaces. To filter results, provide a valid $PropertyNameLower$ (e.g., DELETE /$DomainNamePluralLower$?$PropertyNameLower$=Hello). If no filter is desired, omit the parameter (e.g., DELETE /$DomainNamePluralLower$)."],
                            Samples = ["Hello world"],
                        };

                        yield return (nameof(request.$PropertyName$), errorDetails);
                    }
                    """.Replace("$PropertyName$", propertyName)
                       .Replace("$PropertyNameLower$", propertyNameLower)
                       .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower);

                yield return """
                    if (request.$PropertyName$.IsEmpty())
                    {
                        var errorDetails = new ValidationErrorDetails()
                        {
                            CurrentValue = request.$PropertyName$,
                            Errors = [$"{nameof(request.$PropertyName$)} must not be empty. To filter results, provide a valid $PropertyNameLower$ (e.g., DELETE /$DomainNamePluralLower$?$PropertyNameLower$=Hello). If no filter is desired, omit the parameter (e.g., DELETE /$DomainNamePluralLower$)."],
                            Samples = ["Hello world"],
                        };

                        yield return (nameof(request.$PropertyName$), errorDetails);
                    }
                    """.Replace("$PropertyName$", propertyName)
                       .Replace("$PropertyNameLower$", propertyNameLower)
                       .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower);

                yield break;
            }

            // Query type validation
            yield return """
                if (request.$PropertyName$.IsNull())
                {
                    var errorDetails = new ValidationErrorDetails()
                                       {
                                           CurrentValue = request.$PropertyName$,
                                           Errors = [$"{nameof(request.$PropertyName$)} must not be null."],
                                           Samples = ["Hello world"],
                                       };

                    yield return (nameof(request.$PropertyName$), errorDetails);
                }
                """.Replace("$PropertyName$", propertyName)
                   .Replace("$PropertyNameLower$", propertyNameLower)
                   .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower);

            yield return """
                if (request.$PropertyName$.IsEmpty())
                {
                    var errorDetails = new ValidationErrorDetails()
                                       {
                                           CurrentValue = request.$PropertyName$,
                                           Errors = [$"{nameof(request.$PropertyName$)} must not be empty."],
                                           Samples = ["Hello world"],
                                       };

                    yield return (nameof(request.$PropertyName$), errorDetails);
                }
                """.Replace("$PropertyName$", propertyName)
                   .Replace("$PropertyNameLower$", propertyNameLower)
                   .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower);

            yield return """
                if (request.$PropertyName$.IsWhitespace())
                {
                    var errorDetails = new ValidationErrorDetails()
                                       {
                                           CurrentValue = request.$PropertyName$,
                                           Errors = [$"{nameof(request.$PropertyName$)} must not be one or only whitespaces."],
                                           Samples = ["Hello world"],
                                       };

                    yield return (nameof(request.$PropertyName$), errorDetails);
                }
                """.Replace("$PropertyName$", propertyName)
                   .Replace("$PropertyNameLower$", propertyNameLower)
                   .Replace("$DomainNamePluralLower$", createRestApiInfos.DomainNamePluralLower);
        }
    }
}
