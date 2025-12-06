using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddProblemDetailsCodeGenExtension
    {
        internal static void AddProblemDetailsCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, ProblemDetailsCodeGen>();
        }
    }

    internal sealed class ProblemDetailsCodeGen(ConsoleService consoleService,
                                                NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using System.Diagnostics.CodeAnalysis;
                                        using System.Text.Json;
                                        using System.Text.Json.Serialization;

                                        namespace $namespace$
                                        {
                                            internal sealed class ProblemDetailsJsonConverter : JsonConverter<ProblemDetails>
                                            {
                                                private static readonly JsonEncodedText JsonEncodedType = JsonEncodedText.Encode("type");
                                                private static readonly JsonEncodedText Title = JsonEncodedText.Encode("title");
                                                private static readonly JsonEncodedText Status = JsonEncodedText.Encode("status");
                                                private static readonly JsonEncodedText Detail = JsonEncodedText.Encode("detail");
                                                private static readonly JsonEncodedText Instance = JsonEncodedText.Encode("instance");

                                                [UnconditionalSuppressMessage("Trimmer", "IL2026", Justification = "Trimmer does not allow annotating overriden methods with annotations different from the ones in base type.")]
                                                public override ProblemDetails Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                                                {
                                                    var problemDetails = new ProblemDetails();

                                                    if (reader.TokenType != JsonTokenType.StartObject)
                                                    {
                                                        throw new JsonException("Unexcepted end when reading JSON.");
                                                    }

                                                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                                                    {
                                                        ReadValue(ref reader, problemDetails, options);
                                                    }

                                                    if (reader.TokenType != JsonTokenType.EndObject)
                                                    {
                                                        throw new JsonException("Unexcepted end when reading JSON.");
                                                    }

                                                    return problemDetails;
                                                }

                                                [UnconditionalSuppressMessage("Trimmer", "IL2026", Justification = "Trimmer does not allow annotating overriden methods with annotations different from the ones in base type.")]
                                                public override void Write(Utf8JsonWriter writer, ProblemDetails value, JsonSerializerOptions options)
                                                {
                                                    writer.WriteStartObject();
                                                    WriteProblemDetails(writer, value, options);
                                                    writer.WriteEndObject();
                                                }

                                                [RequiresUnreferencedCode("JSON serialization and deserialization of ProblemDetails.Extensions might require types that cannot be statically analyzed.")]
                                                internal static void ReadValue(ref Utf8JsonReader reader, ProblemDetails value, JsonSerializerOptions options)
                                                {
                                                    if (TryReadStringProperty(ref reader, JsonEncodedType, out var propertyValue))
                                                    {
                                                        value.Type = propertyValue;
                                                    }
                                                    else if (TryReadStringProperty(ref reader, Title, out propertyValue))
                                                    {
                                                        value.Title = propertyValue;
                                                    }
                                                    else if (TryReadStringProperty(ref reader, Detail, out propertyValue))
                                                    {
                                                        value.Detail = propertyValue;
                                                    }
                                                    else if (TryReadStringProperty(ref reader, Instance, out propertyValue))
                                                    {
                                                        value.Instance = propertyValue;
                                                    }
                                                    else if (reader.ValueTextEquals(Status.EncodedUtf8Bytes))
                                                    {
                                                        reader.Read();
                                                        if (reader.TokenType == JsonTokenType.Null)
                                                        {
                                                            // Nothing to do here.
                                                        }
                                                        else
                                                        {
                                                            value.Status = reader.GetInt32();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var key = reader.GetString()!;
                                                        reader.Read();
                                                        value.Extensions[key] = JsonSerializer.Deserialize<object>(ref reader, options);
                                                    }
                                                }

                                                internal static bool TryReadStringProperty(ref Utf8JsonReader reader, JsonEncodedText propertyName, [NotNullWhen(true)] out string? value)
                                                {
                                                    if (!reader.ValueTextEquals(propertyName.EncodedUtf8Bytes))
                                                    {
                                                        value = default;
                                                        return false;
                                                    }

                                                    reader.Read();
                                                    value = reader.GetString()!;
                                                    return true;
                                                }

                                                [RequiresUnreferencedCode("JSON serialization and deserialization of ProblemDetails.Extensions might require types that cannot be statically analyzed.")]
                                                internal static void WriteProblemDetails(Utf8JsonWriter writer, ProblemDetails value, JsonSerializerOptions options)
                                                {
                                                    if (value.Type != null)
                                                    {
                                                        writer.WriteString(JsonEncodedType, value.Type);
                                                    }

                                                    if (value.Title != null)
                                                    {
                                                        writer.WriteString(Title, value.Title);
                                                    }

                                                    if (value.Status != null)
                                                    {
                                                        writer.WriteNumber(Status, value.Status.Value);
                                                    }

                                                    if (value.Detail != null)
                                                    {
                                                        writer.WriteString(Detail, value.Detail);
                                                    }

                                                    if (value.Instance != null)
                                                    {
                                                        writer.WriteString(Instance, value.Instance);
                                                    }

                                                    foreach (var kvp in value.Extensions)
                                                    {
                                                        writer.WritePropertyName(kvp.Key);
                                                        JsonSerializer.Serialize(writer, kvp.Value, kvp.Value?.GetType() ?? typeof(object), options);
                                                    }
                                                }
                                            }
                                            [JsonConverter(typeof(ProblemDetailsJsonConverter))]
                                            internal sealed class ProblemDetails
                                            {
                                                /// <summary>
                                                /// A URI reference [RFC3986] that identifies the problem type. This specification encourages that, when
                                                /// dereferenced, it provide human-readable documentation for the problem type
                                                /// (e.g., using HTML [W3C.REC-html5-20141028]). When this member is not present, its value is assumed to be
                                                /// "about:blank".
                                                /// </summary>
                                                [JsonPropertyName("type")]
                                                internal string? Type { get; set; }

                                                /// <summary>
                                                /// A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence
                                                /// of the problem, except for purposes of localization(e.g., using proactive content negotiation;
                                                /// see[RFC7231], Section 3.4).
                                                /// </summary>
                                                [JsonPropertyName("title")]
                                                internal string? Title { get; set; }

                                                /// <summary>
                                                /// The HTTP status code([RFC7231], Section 6) generated by the origin server for this occurrence of the problem.
                                                /// </summary>
                                                [JsonPropertyName("status")]
                                                internal int? Status { get; set; }

                                                /// <summary>
                                                /// A human-readable explanation specific to this occurrence of the problem.
                                                /// </summary>
                                                [JsonPropertyName("detail")]
                                                internal string? Detail { get; set; }

                                                /// <summary>
                                                /// A URI reference that identifies the specific occurrence of the problem. It may or may not yield further information if dereferenced.
                                                /// </summary>
                                                [JsonPropertyName("instance")]
                                                internal string? Instance { get; set; }

                                                /// <summary>
                                                /// Gets the <see cref="IDictionary{TKey, TValue}"/> for extension members.
                                                /// <para>
                                                /// Problem type definitions MAY extend the problem details object with additional members. Extension members appear in the same namespace as
                                                /// other members of a problem type.
                                                /// </para>
                                                /// </summary>
                                                /// <remarks>
                                                /// The round-tripping behavior for <see cref="Extensions"/> is determined by the implementation of the Input \ Output formatters.
                                                /// In particular, complex types or collection types may not round-trip to the original type when using the built-in JSON or XML formatters.
                                                /// </remarks>
                                                [JsonExtensionData]
                                                internal IDictionary<string, object?> Extensions { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);
                                            }
                                        }
                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add ErrorHandling Folder
            var appFolder = new DirectoryInfo(Path.Combine(projectFileInfo.Directory!.FullName, "ErrorHandling"));

            if (appFolder.NotExists())
            {
                appFolder.Create();
            }

            // 2. Add ProblemDetails.cs
            var file = Path.Combine(appFolder.FullName, "ProblemDetails.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName);

            var formattedTemplate = newTemplate;

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 3. Adjust namespace provider
            namespaceProvider.SetNamespaceProvider(projectFileInfo, $"{dotNetToolInfos.ProjectName}.ErrorHandling", true);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
