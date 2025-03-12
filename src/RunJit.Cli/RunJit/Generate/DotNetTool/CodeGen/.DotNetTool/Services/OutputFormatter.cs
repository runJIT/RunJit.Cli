using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddOutputFormatterCodeGenExtension
    {
        internal static void AddOutputFormatterCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, OutputFormatterCodeGen>();
        }
    }

    internal sealed class OutputFormatterCodeGen(ConsoleService consoleService,
                                                 NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using System.Collections.Immutable;
                                        using System.Text.Json;
                                        using System.Text.Json.Serialization;
                                        using Extensions.Pack;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            internal static class AddOutputFormatterExtension
                                            {
                                                internal static void AddOutputFormatter(this IServiceCollection services)
                                                {
                                                    services.AddJsonFormatterStrategy();
                                                    services.AddJsonIndentedFormatterStrategy();
                                                    services.AddJsonAsStringFormatterStrategy();
                                        
                                                    services.AddSingletonIfNotExists<OutputFormatter>();
                                                }
                                            }
                                        
                                            internal enum FormatType
                                            {
                                                Json,
                                                JsonIndented,
                                                JsonAsString,
                                            }
                                        
                                            internal sealed class OutputFormatter(IEnumerable<IOutputFormatterStrategy> stringFormatterStrategies)
                                            {
                                                internal string Format(string value,
                                                                       FormatType formatType)
                                                {
                                                    // 1. Find the strategy that can handle the format type
                                                    var strategies = stringFormatterStrategies.Where(s => s.CanHandle(formatType)).ToImmutableList();
                                        
                                                    // 2. If we have no matched we throw problem details exception
                                                    if (strategies.IsEmpty())
                                                    {
                                                        throw new ProblemDetailsException("No string formatter strategy was found",
                                                                                          $"No string formatter strategy can handle the specified format type: {formatType}",
                                                                                          ("Format", formatType),
                                                                                          ("String", value));
                                                    }
                                        
                                                    // 3. If we have more than one matched we throw problem details exception
                                                    if (strategies.Count > 1)
                                                    {
                                                        throw new ProblemDetailsException("Multiple string formatter strategies were found",
                                                                                          $"Multiple string formatter strategies can handle the specified format type: {formatType}. Count: {strategies.Count}",
                                                                                          ("Format", formatType),
                                                                                          ("String", value),
                                                                                          ("Strategies", strategies.Select(s => s.GetType().Name)));
                                                    }
                                        
                                                    // 4. If we have one matched we format the value and return it
                                                    return strategies[0].Format(value, formatType);
                                                }
                                            }
                                        
                                            internal interface IOutputFormatterStrategy
                                            {
                                                bool CanHandle(FormatType formatType);
                                        
                                                string Format(string value,
                                                              FormatType formatType);
                                            }
                                        
                                            internal static class AddJsonFormatterStrategyExtension
                                            {
                                                internal static void AddJsonFormatterStrategy(this IServiceCollection services)
                                                {
                                                    services.AddSingletonIfNotExists<IOutputFormatterStrategy, JsonFormatterStrategy>();
                                                }
                                            }
                                        
                                            internal sealed class JsonFormatterStrategy : IOutputFormatterStrategy
                                            {
                                                public bool CanHandle(FormatType formatType)
                                                {
                                                    return formatType == FormatType.Json;
                                                }
                                        
                                                public string Format(string value,
                                                                     FormatType formatType)
                                                {
                                                    // 1. Safety first this method can be called without CanHandle check !
                                                    if (CanHandle(formatType).IsFalse())
                                                    {
                                                        throw new ProblemDetailsException("JsonFormatterStrategy was called without checking CanHandle.",
                                                                                          $"The string formatter strategy: {nameof(JsonFormatterStrategy)} can not process your request",
                                                                                          ("CanHandle", FormatType.Json));
                                                    }
                                        
                                                    // 2. Format incoming JSON string
                                                    using var doc = JsonDocument.Parse(value);
                                        
                                                    // 3. return the formatted json
                                                    return doc.ToJson();
                                                }
                                            }
                                        
                                            internal static class AddJsonIndentedFormatterStrategyExtension
                                            {
                                                internal static void AddJsonIndentedFormatterStrategy(this IServiceCollection services)
                                                {
                                                    services.AddSingletonIfNotExists<IOutputFormatterStrategy, JsonIndentedFormatterStrategy>();
                                                }
                                            }
                                        
                                            internal sealed class JsonIndentedFormatterStrategy : IOutputFormatterStrategy
                                            {
                                                public bool CanHandle(FormatType formatType)
                                                {
                                                    return formatType == FormatType.JsonIndented;
                                                }
                                        
                                                public string Format(string value,
                                                                     FormatType formatType)
                                                {
                                                    // 1. Safety first this method can be called without CanHandle check !
                                                    if (CanHandle(formatType).IsFalse())
                                                    {
                                                        throw new ProblemDetailsException("JsonFormatterStrategy was called without checking CanHandle.",
                                                                                          $"The string formatter strategy: {nameof(JsonIndentedFormatterStrategy)} can not process your request",
                                                                                          ("CanHandle", FormatType.JsonIndented));
                                                    }
                                        
                                                    // 2. Format incoming JSON string
                                                    using var doc = JsonDocument.Parse(value);
                                        
                                                    // 3. Return the formatted json
                                                    return doc.ToJsonIntended();
                                                }
                                            }
                                        
                                            internal static class AddJsonAsStringFormatterStrategyExtension
                                            {
                                                internal static void AddJsonAsStringFormatterStrategy(this IServiceCollection services)
                                                {
                                                    services.AddSingletonIfNotExists<IOutputFormatterStrategy, JsonAsStringFormatterStrategy>();
                                                }
                                            }
                                        
                                            internal sealed class JsonAsStringFormatterStrategy : IOutputFormatterStrategy
                                            {
                                                private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
                                                {
                                                    WriteIndented = false,
                                                    Converters = { new JsonStringEnumConverter() }
                                                };
                                        
                                                public bool CanHandle(FormatType formatType)
                                                {
                                                    return formatType == FormatType.JsonAsString;
                                                }
                                        
                                                public string Format(string value,
                                                                     FormatType formatType)
                                                {
                                                    // 1. Safety first this method can be called without CanHandle check !
                                                    if (CanHandle(formatType).IsFalse())
                                                    {
                                                        throw new ProblemDetailsException("JsonFormatterStrategy was called without checking CanHandle.",
                                                                                          $"The string formatter strategy: {nameof(JsonAsStringFormatterStrategy)} can not process your request",
                                                                                          ("CanHandle", FormatType.JsonAsString));
                                                    }
                                        
                                        
                                                    // 2. Convert into object
                                                    using var doc = JsonDocument.Parse(value);
                                        
                                                    // 3. Serialize into json with specific options
                                                    var json = JsonSerializer.Serialize(doc, _jsonSerializerOptions);
                                        
                                                    // 4. Escape double quotes
                                                    json = json.Replace("\"", "\\\"");
                                        
                                                    // 5. Wrap it in double quotes to produce the desired output
                                                    json = $"\"{json}\"";
                                        
                                                    // 6. Return the formatted json
                                                    return json;
                                                }
                                            }
                                        }

                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add Services Folder
            var appFolder = new DirectoryInfo(Path.Combine(projectFileInfo.Directory!.FullName, "Services"));

            if (appFolder.NotExists())
            {
                appFolder.Create();
            }

            // 2. Add OutputFormatter.cs
            var file = Path.Combine(appFolder.FullName, "OutputFormatter.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName)
                                      .Replace("$dotnettoolnamelower$", dotNetToolInfos.NormalizedName.ToLower());

            var formattedTemplate = newTemplate;

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 3. Adjust namespace provider
            namespaceProvider.SetNamespaceProvider(projectFileInfo, $"{dotNetToolInfos.ProjectName}.Services", true);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
