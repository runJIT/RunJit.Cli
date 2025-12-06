using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.RunJit.Generate.Client
{
    internal static class AddCollectSwaggerPathValidatorExtension
    {
        internal static void AddCollectSwaggerPathValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<CollectSwaggerPathValidator>();
        }
    }

    internal sealed class CollectSwaggerPathValidator : IInputValidator
    {
        public ValidationResult Validate(string value)
        {
            // No argument check here !
            // Throw.IfNullOrWhiteSpace(() => value);

            var errors = CollectErrors(value).Flatten(Environment.NewLine);

            return new ValidationResult(errors);
        }

        private IEnumerable<string> CollectErrors(string value)
        {
            var normalizedValue = value.ToLowerInvariant();

            if (value.IsNullOrWhiteSpace())
            {
                yield return $"The target path: '{value}' must not be null, empty or whitespace";

                yield break;
            }

            if (value.Contains(" "))
            {
                yield return $"The target path: '{value}' must not contains whitespace";

                yield break;
            }

            // we got url to fetch swagger json by http client
            if (value.StartWith("http"))
            {
                if (Uri.TryCreate(normalizedValue, UriKind.Absolute, out var uriResult) && (uriResult.Scheme.EqualsTo(Uri.UriSchemeHttp) || uriResult.Scheme.EqualsTo(Uri.UriSchemeHttps)).IsFalse())
                {
                    yield return $"Your uri path to fetch swagger: '{value}' is not valid. Please use correct uri path.";

                    yield break;
                }
            }

            if (Path.IsPathFullyQualified(value).IsFalse())
            {
                yield return $"The target path: '{value}' for the swagger file is not valid";
            }

            if (File.Exists(value).IsFalse())
            {
                yield return $"The swagger file: '{value}' does not exists";
            }
        }
    }
}
