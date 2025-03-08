using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RunJit.Cli.Services
{
    // Introduce an enum to categorize the type of mismatch.
    public enum MismatchType
    {
        ValueDifference,   // Both values exist but are not equal.
        MissingInFirst,    // The value is missing in the first JSON.
        MissingInSecond    // The value is missing in the second JSON.
    }

    // Update the Difference record to include the mismatch type.
    public sealed record Difference(string MemberPath,
                                       string? Value1,
                                       string? Value2,
                                       MismatchType MismatchType);

    public static class AddJsonSerializationExtensions
    {
        public static void AddJsonDiffer(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IJsonDiffer, JsonDiffer>();
        }
    }

    public interface IJsonDiffer
    {
        IImmutableList<Difference> FindDifferences(string json1,
                                                   string json2);

        // Updated native differences method to include mismatch type.
        Dictionary<string, (JToken?, JToken?, MismatchType)> FindDifferencesNative(string json1,
                                                                                    string json2);
    }

    internal sealed class JsonDiffer : IJsonDiffer
    {
        public IImmutableList<Difference> FindDifferences(string json1,
                                                          string json2)
        {
            var differences = FindDifferencesNative(json1, json2);

            var simpleDifferences = differences.Select(item =>
                new Difference(
                    item.Key,
                    item.Value.Item1?.ToString(),
                    item.Value.Item2?.ToString(),
                    item.Value.Item3));

            return simpleDifferences.ToImmutableList();
        }

        public Dictionary<string, (JToken?, JToken?, MismatchType)> FindDifferencesNative(string json1,
                                                                                            string json2)
        {
            var differences = new Dictionary<string, (JToken?, JToken?, MismatchType)>();

            CompareTokens(JToken.Parse(json1), JToken.Parse(json2), differences, "");

            return differences;
        }

        private void CompareTokens(JToken? token1,
                                   JToken? token2,
                                   Dictionary<string, (JToken?, JToken?, MismatchType)> differences,
                                   string path)
        {
            if (JToken.DeepEquals(token1, token2))
            {
                return;
            }

            // Handle cases where one token is missing.
            if (token1 == null || token1.IsNull())
            {
                differences[path] = (null, token2, MismatchType.MissingInFirst);
                return;
            }

            if (token2 == null || token2.IsNull())
            {
                differences[path] = (token1, null, MismatchType.MissingInSecond);
                return;
            }

            switch (token1.Type)
            {
                case JTokenType.Object:
                    if (token2.Type != JTokenType.Object)
                    {
                        differences[path] = (token1, token2, MismatchType.ValueDifference);
                        return;
                    }

                    var obj1 = (JObject)token1;
                    var obj2 = (JObject)token2;

                    // Compare all properties from the first object.
                    foreach (var property in obj1)
                    {
                        var propertyPath = AppendPath(path, property.Key);
                        var token2Value = obj2.GetValueOrDefault(property.Key);

                        if (token2Value == null)
                        {
                            differences[propertyPath] = (property.Value, null, MismatchType.MissingInSecond);
                        }
                        else
                        {
                            CompareTokens(property.Value, token2Value, differences, propertyPath);
                        }
                    }

                    // Look for properties that are in the second object but not in the first.
                    foreach (var property in obj2)
                    {
                        var propertyPath = AppendPath(path, property.Key);

                        if (obj1[property.Key] == null)
                        {
                            differences[propertyPath] = (null, property.Value, MismatchType.MissingInFirst);
                        }
                    }

                    break;

                case JTokenType.Array:
                    if (token2.Type != JTokenType.Array)
                    {
                        differences[path] = (token1, token2, MismatchType.ValueDifference);
                        return;
                    }

                    var array1 = (JArray)token1;
                    var array2 = (JArray)token2;

                    // Check if the array represents key-value pairs.
                    var isKeyValueArray = array1.Count > 0 &&
                                          array1.First is JObject firstElement &&
                                          firstElement.ContainsKey("Key");

                    for (var i = 0; i < array1.Count || i < array2.Count; i++)
                    {
                        string indexPath;

                        if (isKeyValueArray)
                        {
                            // Use the key value if available.
                            var key1 = i < array1.Count ? array1[i]["Key"]?.ToString() : null;
                            var key2 = i < array2.Count ? array2[i]["Key"]?.ToString() : null;
                            indexPath = AppendPath(path, $"""["{key1 ?? key2 ?? i.ToString()}"]""");
                        }
                        else
                        {
                            indexPath = AppendPath(path, $"[{i}]");
                        }

                        if (i >= array1.Count)
                        {
                            differences[indexPath] = (null, array2[i], MismatchType.MissingInFirst);
                        }
                        else if (i >= array2.Count)
                        {
                            differences[indexPath] = (array1[i], null, MismatchType.MissingInSecond);
                        }
                        else
                        {
                            CompareTokens(array1[i], array2[i], differences, indexPath);
                        }
                    }

                    break;

                default:
                    // For primitive types, record the difference as a value difference.
                    differences[path] = (token1, token2, MismatchType.ValueDifference);
                    break;
            }
        }

        private string AppendPath(string path,
                                  string addition)
        {
            if (string.IsNullOrEmpty(path))
            {
                return addition;
            }

            // If the addition represents an array index, don't add a dot.
            if (addition.First() == '[')
            {
                return $"{path}{addition}";
            }

            return $"{path}.{addition}";
        }
    }
}
