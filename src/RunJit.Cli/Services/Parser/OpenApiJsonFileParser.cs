using System.Collections.Immutable;
using System.Text.Json.Nodes;
using AspNetCore.Simple.MsTest.Sdk;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using RunJit.Cli.Services.Endpoints;
using Solution.Parser.CSharp;
using Attribute = Solution.Parser.CSharp.Attribute;
using Enum = Solution.Parser.CSharp.Enum;

namespace RunJit.Cli.Services
{
    internal static class AddOpenApiJsonFileParserExtension
    {
        internal static void AddOpenApiJsonFileParser(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<OpenApiJsonFileParser>();
        }
    }

    internal sealed class OpenApiJsonFileParser
    {
        // Root namespace for generated Records (can be changed later)
        private const string DefaultGeneratedNamespace = "Sdc.Console.Generated.OpenApi";

        // Holds all parsed OpenAPI components/schemas as Records
        private ImmutableDictionary<string, Record> _componentRecords =
            ImmutableDictionary<string, Record>.Empty;

        /// <summary>
        ///     Main entrypoint: parses an OpenAPI JSON wrapper and returns all EndpointInfos.
        /// </summary>
        public ImmutableList<EndpointInfo> ExtractFrom(string basePath,
                                                       ImmutableList<CSharpSyntaxTree> syntaxTrees, // currently unused, kept for compatibility
                                                       ImmutableList<Type> reflectionTypes, // currently unused, kept for compatibility
                                                       FileInfo openApiJsonFileInfo)
        {
            var fileContent = File.ReadAllText(openApiJsonFileInfo.FullName);

            // You said your JSON is wrapped as SimpleHttpResponseMessage
            var simple = fileContent.FromJsonStringAs<SimpleHttpResponseMessage>();
            var jsonObj = JsonNode.Parse(simple.Content?.Value?.ToString() ?? "{}");

            // Parse OpenAPI
            var openApiDoc = new OpenApiStringReader().Read(jsonObj!.ToJsonString(), out var diag);

            // 1) Parse all component schemas into Records
            _componentRecords = ParseAllComponentSchemas(openApiDoc, DefaultGeneratedNamespace);

            // 2) Convert paths/operations into EndpointInfo
            var result = ConvertOpenApiToEndpointInfos(openApiDoc, basePath);

            return result;
        }

        // -------------------------------------------------------------------------
        // 1. PARSE COMPONENTS/SCHEMAS → Record dictionary
        // -------------------------------------------------------------------------
        private ImmutableDictionary<string, Record> ParseAllComponentSchemas(OpenApiDocument doc,
                                                                             string rootNamespace)
        {
            var dict = new Dictionary<string, Record>(StringComparer.OrdinalIgnoreCase);

            foreach (var (name, schema) in doc.Components.Schemas)
            {
                ConvertSchemaToRecord(name, schema, rootNamespace,
                                      dict);
            }

            return dict.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Converts a single schema (and any inline nested object schemas) into Records.
        ///     All created Records are stored in the <paramref name="bucket" />.
        /// </summary>
        private Record ConvertSchemaToRecord(string name,
                                             OpenApiSchema schema,
                                             string rootNamespace,
                                             Dictionary<string, Record> bucket)
        {
            // If this schema was already converted, just return it
            if (bucket.TryGetValue(name, out var existing))
            {
                return existing;
            }

            var propertiesBuilder = ImmutableList.CreateBuilder<Property>();

            // Build properties
            foreach (var (propName, propSchema) in schema.Properties)
            {
                var propertyType = ResolvePropertyType(name,
                                                       propName,
                                                       propSchema,
                                                       rootNamespace,
                                                       bucket);

                var normalizedPropertyName = propName.FirstCharToUpper();

                // Until we know how to get the required info
                var isRequired = propSchema.Nullable.IsFalse();
                var required = isRequired ? "required " : string.Empty;
                var nullableNotation = propSchema.Nullable ? "?" : string.Empty;

                var prop = new Property(propertyType,
                                        normalizedPropertyName,
                                        true,
                                        isRequired,
                                        propSchema.Nullable,
                                        isRequired ? [Modifier.Public, Modifier.Required] : [Modifier.Public],
                                        ImmutableList<Attribute>.Empty,
                                        $"public {required}{propertyType}{nullableNotation} {normalizedPropertyName} {{ get; init; }}",
                                        string.Empty,
                                        string.Empty);

                propertiesBuilder.Add(prop);
            }

            // Build Record itself
            var ns = new NameSpace(rootNamespace);
            var fullName = $"{rootNamespace}.{name}";

            var record = new Record(ns,
                                    name,
                                    [Modifier.Public],
                                    ImmutableList<Constructor>.Empty,
                                    propertiesBuilder.ToImmutable(),
                                    ImmutableList<Method>.Empty,
                                    ImmutableList<Attribute>.Empty,
                                    ImmutableList<Field>.Empty,
                                    ImmutableList<Interface>.Empty,
                                    ImmutableList<BaseType>.Empty,
                                    ImmutableList<Event>.Empty,
                                    ImmutableList<EventField>.Empty,
                                    ImmutableList<Class>.Empty,
                                    ImmutableList<Struct>.Empty,
                                    ImmutableList<Enum>.Empty,
                                    ImmutableList<Interface>.Empty,
                                    ImmutableList<Parameter>.Empty,
                                    fullName,
                                    string.Empty,
                                    string.Empty);

            record = ToRecordSyntaxTree(record);

            bucket[name] = record;

            return record;
        }

        private Record ToRecordSyntaxTree(Record record)
        {
            var properties = record.Properties.Select(p => p.SyntaxTree).Flatten(Environment.NewLine);

            var syntaxTree = @$"public sealed record {record.Name}
                               {{
                                    {properties}
                               }}
                               ".FormatSyntaxTree();

            var newRecord = record with { SyntaxTree = syntaxTree };

            return newRecord;
        }

        /// <summary>
        ///     Resolves a schema’s type into a string and creates nested Records for inline object schemas.
        /// </summary>
        private string ResolvePropertyType(string containingTypeName,
                                           string propertyName,
                                           OpenApiSchema schema,
                                           string rootNamespace,
                                           Dictionary<string, Record> bucket)
        {
            // $ref → components/schemas/Name
            if (schema.Reference != null && !schema.Reference.Id.IsNullOrWhiteSpace())
            {
                var refName = schema.Reference.Id;

                // Make sure we also parsed that referenced schema if available
                if (bucket.TryGetValue(refName, out _))
                {
                    // already known
                }

                // if it's not in the bucket yet but exists in document.Component.Schemas,
                // it will be added there during ParseAllComponentSchemas
                return refName;
            }

            // Arrays → List<T> (or ImmutableList<T> if you prefer)
            if (string.Equals(schema.Type, "array", StringComparison.OrdinalIgnoreCase))
            {
                var itemSchema = schema.Items ?? new OpenApiSchema { Type = "object" };

                var innerType = ResolvePropertyType(containingTypeName,
                                                    propertyName + "Item",
                                                    itemSchema,
                                                    rootNamespace,
                                                    bucket);

                return $"ImmutableList<{innerType}>";
            }

            // Inline object → create a nested Record with synthetic name: Parent_Property
            if (string.Equals(schema.Type, "object", StringComparison.OrdinalIgnoreCase)
                && schema.Properties is { Count: > 0 })
            {
                var nestedName = $"{containingTypeName}_{propertyName}";

                ConvertSchemaToRecord(nestedName, schema, rootNamespace,
                                      bucket);

                return nestedName;
            }

            // Primitive types
            return MapPrimitiveType(schema.Type, schema.Format);
        }

        private static string MapPrimitiveType(string? type,
                                               string? format)
        {
            if (type.IsNullOrWhiteSpace())
            {
                return "object";
            }

            var result = type switch
            {
                "integer" when string.Equals(format, "int64", StringComparison.OrdinalIgnoreCase) => "long",
                "integer" => "int",
                "number" when string.Equals(format, "double", StringComparison.OrdinalIgnoreCase) => "double",
                "number" => "decimal",
                "boolean" => "bool",
                "string" when string.Equals(format, "date-time", StringComparison.OrdinalIgnoreCase) => "DateTimeOffset",
                "string" when string.Equals(format, "date", StringComparison.OrdinalIgnoreCase) => "DateTime",
                "string" when string.Equals(format, "uuid", StringComparison.OrdinalIgnoreCase) => "Guid",
                "string" => "string",
                _ => type
            };

            return result;
        }

        // -------------------------------------------------------------------------
        // 2. CONVERT OpenApiDocument → EndpointInfo list
        // -------------------------------------------------------------------------
        private ImmutableList<EndpointInfo> ConvertOpenApiToEndpointInfos(OpenApiDocument doc,
                                                                          string basePath)
        {
            var builder = ImmutableList.CreateBuilder<EndpointInfo>();

            foreach (var (openApiPath, pathItem) in doc.Paths)
            {
                foreach (var (operationType, operation) in pathItem.Operations)
                {
                    var httpMethod = operationType.ToString().ToUpperInvariant();

                    var extractRequestType = ExtractRequestType(operation);
                    var extractResponseType = ExtractResponseType(operation);
                    var allModels = GetAllModels(extractResponseType, extractRequestType).ToImmutableList();

                    var endpoint = new EndpointInfo
                                   {
                                       Name = ExtractNameFromOperation(operation),
                                       DomainName = ExtractDomainNameFromOperationId(operation.OperationId),
                                       Version = ExtractVersionFromPath(openApiPath),
                                       BaseUrl = basePath,
                                       GroupName = ExtractGroupName(operation),
                                       SwaggerOperationId = operation.OperationId ?? string.Empty,
                                       HttpAction = httpMethod,
                                       RelativeUrl = BuildRelativeUrl(basePath, openApiPath),
                                       Parameters = ExtractParameters(operation),
                                       RequestType = extractRequestType,
                                       ResponseType = extractResponseType,
                                       ProduceResponseTypes = ExtractProduceResponseTypes(operation),
                                       Models = allModels,
                                       ObsoleteInfo = ExtractObsolete(operation)
                                   };

                    builder.Add(endpoint);
                }
            }

            return builder.ToImmutable();
        }

        private IEnumerable<DeclarationBase> GetAllModels(ResponseType extractResponseType,
                                                          RequestType? extractRequestType)
        {
            // ToDo we have to collect it recursivly
            if (extractRequestType.IsNotNull())
            {
                yield return extractRequestType.Declaration;

                var allRequestTypeModels = FindAllTypesForDeclaration(extractRequestType.Declaration);
                foreach (var allRequestTypeModel in allRequestTypeModels)
                {
                    yield return allRequestTypeModel;
                }
            }

            if (_componentRecords.TryGetValue(extractResponseType.Original, out var type))
            {
                yield return type;

                var allResponseModels = FindAllTypesForDeclaration(type);
                foreach (var responseModel in allResponseModels)
                {
                    yield return responseModel;
                }
            }
        }


        private IEnumerable<Record> FindAllTypesForDeclaration(DeclarationBase declarationBase)
        {
            var propertyType = declarationBase.As<Interface>();
            if (propertyType.IsNull())
            {
                yield break;
            }

            foreach (var propertyTypeProperty in propertyType.Properties)
            {
                if (_componentRecords.TryGetValue(propertyTypeProperty.Type, out var type))
                {
                    var allRecords = FindAllTypesForDeclaration(type);
                    foreach (var record in allRecords)
                    {
                        yield return record;
                    }
                }
            }
        }

        private string ExtractNameFromOperation(OpenApiOperation op)
        {
            if (!op.OperationId.IsNullOrWhiteSpace())
            {
                return op.OperationId;
            }

            return op.Summary ?? "UnnamedEndpoint";
        }

        private string ExtractDomainNameFromOperationId(string? operationId)
        {
            if (operationId.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            var idx = operationId.IndexOf('_');

            return idx > 0 ? operationId[..idx] : operationId;
        }

        private VersionInfo ExtractVersionFromPath(string path)
        {
            var segment = path.Split('/', StringSplitOptions.RemoveEmptyEntries)
                              .FirstOrDefault(s => s.StartWith("v"));

            if (segment.IsNull())
            {
                return new VersionInfo("1.0", "V1");
            }

            var versionNumber = segment.TrimStart('v', 'V');

            return new VersionInfo(versionNumber, "V" + versionNumber);
        }

        private string ExtractGroupName(OpenApiOperation op)
        {
            return op.Tags?.FirstOrDefault()?.Name ?? "Default";
        }

        private string BuildRelativeUrl(string basePath,
                                        string openApiPath)
        {
            if (openApiPath.StartWith(basePath))
            {
                return openApiPath.Substring(basePath.Length).TrimStart('/');
            }

            return openApiPath.TrimStart('/');
        }

        private ImmutableList<Parameter> ExtractParameters(OpenApiOperation op)
        {
            var builder = ImmutableList.CreateBuilder<Parameter>();

            foreach (var p in op.Parameters)
            {
                var schemaType = p.Schema?.Type ?? "string";

                var parameter = new Parameter(schemaType, p.Name, ImmutableList<Attribute>.Empty,
                                              string.Empty, true, null,
                                              string.Empty);

                builder.Add(parameter);
            }

            return builder.ToImmutable();
        }

        private RequestType? ExtractRequestType(OpenApiOperation op)
        {
            if (op.RequestBody.IsNull())
            {
                return null;
            }

            var jsonSchema = op.RequestBody.Content
                               .Where(kvp => kvp.Key.Contains("json", StringComparison.OrdinalIgnoreCase))
                               .Select(kvp => kvp.Value.Schema)
                               .FirstOrDefault();

            if (jsonSchema.IsNull())
            {
                return null;
            }

            var typeName = jsonSchema.Reference?.Id
                           ?? jsonSchema.Type
                           ?? "UnknownRequest";

            if (_componentRecords.TryGetValue(typeName, out var declaredType))
            {
                return new RequestType(declaredType, typeName);
            }

            return null;
        }

        private ResponseType ExtractResponseType(OpenApiOperation op)
        {
            var response =
                op.Responses.TryGetValue("200", out var r200) ? r200 :
                op.Responses.TryGetValue("201", out var r201) ? r201 :
                op.Responses.Values.FirstOrDefault();

            var schema = response?.Content?
                .Values?
                .FirstOrDefault()?
                .Schema;

            var typeName = schema?.Reference?.Id
                           ?? schema?.Type
                           ?? "UnknownResponse";

            if (_componentRecords.TryGetValue(typeName, out var declaredType))
            {
                return new ResponseType(declaredType.Name, typeName);
            }

            return new ResponseType("void", "void");
        }

        private ImmutableList<ProduceResponseTypes> ExtractProduceResponseTypes(OpenApiOperation op)
        {
            var builder = ImmutableList.CreateBuilder<ProduceResponseTypes>();

            foreach (var (status, response) in op.Responses)
            {
                var schema = response.Content?
                    .Values.FirstOrDefault()?
                    .Schema;

                var typeName = schema?.Reference?.Id
                               ?? schema?.Type
                               ?? "Unknown";

                _componentRecords.TryGetValue(typeName, out var declaredType);

                var produceResponseTypes = new ProduceResponseTypes(typeName, status.ToIntOrDefault());

                builder.Add(produceResponseTypes);
            }

            return builder.ToImmutable();
        }

        private ObsoleteInfo? ExtractObsolete(OpenApiOperation op)
        {
            return null;
        }
    }
}
