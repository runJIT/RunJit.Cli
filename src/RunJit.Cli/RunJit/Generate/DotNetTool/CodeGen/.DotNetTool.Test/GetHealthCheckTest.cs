using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;
using Solution.Parser.Project;

namespace RunJit.Cli.Generate.DotNetTool.DotNetTool.Test
{
    internal static class AddGetHealthCheckTestCodeGenExtension
    {
        internal static void AddGetHealthCheckTestCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolTestSpecificCodeGen, GetHealthCheckTestCodeGen>();
        }
    }

    // Exception test that health check is working out of the box
    internal sealed class GetHealthCheckTestCodeGen(ConsoleService consoleService) : IDotNetToolTestSpecificCodeGen
    {
        private const string Template = """
                                        using System.Collections.Immutable;
                                        using AspNetCore.Simple.MsTest.Sdk;
                                        using Extensions.Pack;

                                        namespace $namespace$.$dotNetToolName$.Health.GetHealthStatus
                                        {
                                            [TestClass]
                                            [TestCategory("$dotNetToolName$")]
                                            [TestCategory("$dotNetToolName$ Health GetHealthStatus")]
                                            public class GetHealthStatusTest : GlobalSetup
                                            {
                                                /// <summary>
                                                /// Tests the '$dotNetToolNameLower$ health gethealthstatus' command with different output formats and writes the output to a specified file.
                                                /// Asserts the output against the expected JSON file.
                                                /// </summary>
                                                /// <returns>A task representing the asynchronous operation.</returns>
                                                [TestMethod]
                                                public Task $dotNetToolName$_Health_GetHealthStatus()
                                                {
                                                    // 1. Executes the CLI command and asserts the output against the expected JSON file.
                                                    return Cli.AssertRunAsync($"$dotNetToolNameLower$ health gethealthstatus",
                                                                              "GetHealthStatus.json", 
                                                                              differenceFunc:IgnoreValues);
                                                }

                                                /// <summary>
                                                /// Tests the '$dotNetToolNameLower$ health gethealthstatus' command with different output formats and writes the output to a specified file.
                                                /// Asserts the output against the expected JSON file.
                                                /// </summary>
                                                /// <param name="format">The format in which the configuration should be output (e.g., Json, JsonIndented, JsonAsString).</param>
                                                /// <returns>A task representing the asynchronous operation.</returns>
                                                [DataTestMethod]
                                                [DataRow("Json")]
                                                [DataRow("JsonIndented")]
                                                [DataRow("JsonAsString")]
                                                public Task $dotNetToolName$_Health_GetHealthStatus_To_Console_Out_With_Format(string format)
                                                {
                                                    // 1. Executes the CLI command and asserts the output against the expected JSON file.
                                                    return Cli.AssertRunAsync($"$dotNetToolNameLower$ health gethealthstatus --format {format}",
                                                                              $"GetHealthStatusAs{format}.json",
                                                                              IgnoreValues);
                                                }
                                                /// <summary>
                                                /// Tests the '$dotNetToolNameLower$ health gethealthstatus' command with different output formats and writes the output to a specified file.
                                                /// Asserts the output against the expected JSON file.
                                                /// </summary>
                                                /// <param name="format">The format in which the configuration should be output (e.g., Json, JsonIndented, JsonAsString).</param>
                                                /// <returns>A task representing the asynchronous operation.</returns>
                                                [DataTestMethod]
                                                [DataRow("Json")]
                                                [DataRow("JsonIndented")]
                                                [DataRow("JsonAsString")]
                                                public Task $dotNetToolName$_Health_GetHealthStatus_To_File_Output_With_Format(string format)
                                                {
                                                    // 1. Constructs the full path to the output file.
                                                    var file = Path.Combine(Environment.CurrentDirectory, "$dotNetToolName$", "Health", "GetHealthStatus", $"OutputAs{format}.json");
                                                
                                                    // 2. Executes the CLI command and asserts the output against the expected JSON file.
                                                    return Cli.AssertRunAsync($"$dotNetToolNameLower$ health gethealthstatus --format {format} --output {file}",
                                                                              $"GetHealthStatusAs{format}.json",
                                                                              IgnoreValues);
                                                }

                                                private IEnumerable<Difference> IgnoreValues(IImmutableList<Difference> arg)
                                                {
                                                    foreach (var difference in arg)
                                                    {
                                                        if (difference.MemberPath.IsNullOrWhiteSpace())
                                                        {
                                                            continue;
                                                        }

                                                        if (difference.MemberPath.Contains("totalDuration"))
                                                        {
                                                            continue;
                                                        }

                                                        yield return difference;
                                                    }
                                                }
                                            }
                                        }

                                        """;

        private const string expectedJsonOutput = """
                                                  {
                                                    "status": "Healthy",
                                                    "totalDuration": "00:00:00.0000015",
                                                    "entries": {}
                                                  }
                                                  """;

        private const string expectedJsonAsStringOutput = """
                                                          "{\u0022status\u0022:\u0022Healthy\u0022,\u0022totalDuration\u0022:\u002200:00:00.0000015\u0022,\u0022entries\u0022:{}}"
                                                          """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos,
                                        ProjectFile? webApiProject)
        {
            // 1. GlobalSetup
            // Find health status test
            var healthCheckTest = projectFileInfo.Directory!.EnumerateFiles("GetHealthStatusTest.cs", SearchOption.AllDirectories).FirstOrDefault();

            if (healthCheckTest.IsNull())
            {
                return;
            }

            var newTemplate = Template.Replace("$namespace$", $"{dotNetToolInfos.ProjectName}.Test")
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName)
                                      .Replace("$dotNetToolNameLower$", dotNetToolInfos.NormalizedName.ToLower())
                                      .Replace("$webApiProjectName$", webApiProject?.ProjectFileInfo.FileNameWithoutExtenion);

            var formattedTemplate = newTemplate.FormatSyntaxTree();

            await File.WriteAllTextAsync(healthCheckTest.FullName, formattedTemplate).ConfigureAwait(false);

            // 2. Print success message
            consoleService.WriteSuccess($"Successfully created {healthCheckTest.FullName}");

            // Update responses with expected results
            var jsonFiles = projectFileInfo.Directory!.EnumerateFiles("GetHealthStatus*.json", SearchOption.AllDirectories).ToList();

            foreach (var jsonFile in jsonFiles)
            {
                if (jsonFile.Name.Contains("JsonAsString"))
                {
                    await File.WriteAllTextAsync(jsonFile.FullName, expectedJsonAsStringOutput).ConfigureAwait(false);
                }
                else
                {
                    await File.WriteAllTextAsync(jsonFile.FullName, expectedJsonOutput).ConfigureAwait(false);
                }

                consoleService.WriteSuccess($"Successfully udated test output file {jsonFile.FullName}");
            }
        }
    }
}
