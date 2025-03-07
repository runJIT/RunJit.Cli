using System.Text;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using Solution.Parser.CSharp;

namespace RunJit.Cli.RunJit.New.RestMinimalApi.Service
{
    public static class AddGenerateMigrationScriptExtension
    {
        public static void AddGenerateMigrationScript(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<GenerateMigrationScript>();
        }
    }

    internal class GenerateMigrationScript
    {
        internal string Generate(Record record)
        {
            var tableAttribute = record.Attributes.FirstOrDefault(a => a.Name.Contains("DynamoDBTable"));
            if (tableAttribute.IsNull())
            {

                var sample = """
                             [DynamoDBTable("Project")]
                             public record ProjectEntity
                             {
                                 [DynamoDBHashKey]
                                 public Guid ProjectId { get; init; } = Guid.Empty;
                             
                                 public string Name { get; init; } = string.Empty;
                             
                                 public string Description { get; init; } = string.Empty;
                             }
                             """;

                throw new RunJitException($"Your provided record type does not have a mandatory [DynamoDBTable(\"Project\")] attribute. Sample: {Environment.NewLine}{sample}");
            }

            var tableName = tableAttribute.Arguments.FirstOrDefault();
            if (tableName.IsNull())
            {

                var sample = """
                             [DynamoDBTable("Project")]
                             public record ProjectEntity
                             {
                                 [DynamoDBHashKey]
                                 public Guid ProjectId { get; init; } = Guid.Empty;
                             
                                 public string Name { get; init; } = string.Empty;
                             
                                 public string Description { get; init; } = string.Empty;
                             }
                             """;

                throw new RunJitException($"Your provided record has a [DynamoDBTable()] but th Sample: {Environment.NewLine}{sample}");
            }

            // 1. Get hashkey
            var hashKey = record.Properties.FirstOrDefault(p => p.SyntaxTree.Contains("DynamoDBHashKey"));
            if (hashKey == null)
            {
                var sample = """
                             [DynamoDBTable("Project")]
                             public record ProjectEntity
                             {
                                 [DynamoDBHashKey]
                                 public Guid ProjectId { get; init; } = Guid.Empty;
                             
                                 public string Name { get; init; } = string.Empty;
                             
                                 public string Description { get; init; } = string.Empty;
                             }
                             """;

                throw new RunJitException($"Your provided record type does not have a mandatory [DynamoDBHashKey] attribute. Sample: {Environment.NewLine}{sample}");
            }

            // Optionally find a range key property
            var rangeKey = record.Properties.FirstOrDefault(p => p.SyntaxTree.Contains("DynamoDBRangeKey"));

            // Build attribute definitions
            var attributeDefs = new List<string>
        {
            $"AttributeName={hashKey.Name},AttributeType={MapType(hashKey.Type)}"
        };

            // Add range key definition if exists
            if (rangeKey != null)
            {
                attributeDefs.Add($"AttributeName={rangeKey.Name},AttributeType={MapType(rangeKey.Type)}");
            }

            // Build key schema definitions
            var keySchema = new List<string>
        {
            $"AttributeName={hashKey.Name},KeyType=HASH"
        };

            if (rangeKey != null)
            {
                keySchema.Add($"AttributeName={rangeKey.Name},KeyType=RANGE");
            }

            // Construct the AWS CLI command
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("aws dynamodb create-table \\");
            stringBuilder.AppendLine($"  --table-name {tableName.Trim('"')} \\");
            stringBuilder.AppendLine($"  --attribute-definitions {string.Join(" ", attributeDefs)} \\");
            stringBuilder.AppendLine($"  --key-schema {string.Join(" ", keySchema)} \\");
            stringBuilder.AppendLine("  --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \\");
            stringBuilder.AppendLine("  --endpoint-url http://localhost:8001");

            var shellCommand = stringBuilder.ToString();

            return shellCommand;
        }

        private string MapType(string typeName)
        {
            if (typeName == "string" || typeName == nameof(Guid))
                return "S";
            if (typeName == "int" || typeName == "long" || typeName == "decimal" || typeName == "double")
                return "N";
            // Default to string
            return "S";
        }
    }
}
