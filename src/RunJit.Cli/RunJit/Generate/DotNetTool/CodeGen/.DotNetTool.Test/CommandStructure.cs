using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool.Models;
using RunJit.Cli.Services;
using Solution.Parser.Project;

namespace RunJit.Cli.Generate.DotNetTool.DotNetTool.Test
{
    internal static class AddCommandStructureCodeGenExtension
    {
        internal static void AddCommandStructureCodeGen(this IServiceCollection services)
        {
            services.AddDefaultTestCodeGen();
            services.AddOutputToConsoleCodeGen();
            services.AddOutputToFileCodeGen();

            services.AddSingletonIfNotExists<IDotNetToolTestSpecificCodeGen, CommandStructureCodeGen>();
        }
    }

    internal sealed class CommandStructureCodeGen(ConsoleService consoleService,
                                                  IEnumerable<IDotNetToolTestCaseCodeGen> dotNetToolTestCaseGenerators) : IDotNetToolTestSpecificCodeGen
    {
        private const string Template = """
                                        namespace $namespace$
                                        {
                                            [TestClass]
                                            [TestCategory("$testCategory$")]
                                            [TestCategory("$commandTestCategory$")]
                                            public class $commandName$Test : GlobalSetup
                                            {
                                        $testCases$
                                            }
                                        }
                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos,
                                        ProjectFile? webApiProject)
        {
            await CreateTestsForCommandAsync(projectFileInfo, projectDocument, dotNetToolInfos,
                                             dotNetToolInfos.CommandInfo, null, string.Empty);

            consoleService.WriteSuccess("Successfully created cli test structure");
        }

        private async Task CreateTestsForCommandAsync(FileInfo projectFileInfo,
                                                      XDocument projectDocument,
                                                      DotNetToolInfos dotNetToolInfos,
                                                      CommandInfo commandInfo,
                                                      DirectoryInfo? parentDirectory,
                                                      string cliCallPath)
        {
            // Create folder
            var folderPath = parentDirectory.IsNull() ? Path.Combine(projectFileInfo.Directory!.FullName, dotNetToolInfos.CommandInfo.NormalizedName) : Path.Combine(parentDirectory.FullName, commandInfo.NormalizedName);

            var targetFolder = new DirectoryInfo(folderPath);

            if (targetFolder.NotExists())
            {
                targetFolder.Create();
            }

            cliCallPath = $"{cliCallPath} {commandInfo.NormalizedName}".TrimStart(' ');

            // If we have an endpoint we create test cases
            if (commandInfo.EndpointInfo.IsNotNull())
            {
                var testCases = new List<string>();

                foreach (var dotNetToolTestCaseGenerator in dotNetToolTestCaseGenerators)
                {
                    var testCase = await dotNetToolTestCaseGenerator.GenerateAsync(dotNetToolInfos, commandInfo, cliCallPath);
                    testCases.Add(testCase);
                }

                var testCasesAsString = testCases.ToFlattenString(Environment.NewLine);
                var testMethodName = cliCallPath.Split(" ").Where(value => value.StartsWith("-").IsFalse()).Flatten("_");
                var parametersFile = cliCallPath.Split(" ").Where(value => value.StartsWith("-").IsFalse()).Flatten(".");
                parametersFile = $"{parametersFile}.Parameters.{commandInfo.NormalizedName}.json";

                var cliCall = cliCallPath;
                var cliCallWithArgument = commandInfo.EndpointInfo.IsNull() ? $"{cliCall}" : $"{cliCall} {parametersFile}";
                var commandTestCategory = cliCallPath;

                var newTemplate = Template.Replace("$namespace$", $"{dotNetToolInfos.ProjectName}.Test")
                                          .Replace("$cliCall$", cliCallWithArgument.ToLower())
                                          .Replace("$testMethodName$", testMethodName)
                                          .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName.ToLower())
                                          .Replace("$commandName$", commandInfo.NormalizedName)
                                          .Replace("$parametersFile$", parametersFile)
                                          .Replace("$testCases$", testCasesAsString)
                                          .Replace("$commandTestCategory$", commandTestCategory)
                                          .Replace("$testCategory$", dotNetToolInfos.NormalizedName);

                await File.WriteAllTextAsync(Path.Combine(targetFolder.FullName, $"{commandInfo.NormalizedName}Test.cs"), newTemplate).ConfigureAwait(false);

                // Output folder
                var outputFolder = new DirectoryInfo(Path.Combine(targetFolder.FullName, "Output"));

                if (outputFolder.NotExists())
                {
                    outputFolder.Create();
                }

                // Default test case
                await File.WriteAllTextAsync(Path.Combine(outputFolder.FullName, $"{commandInfo.NormalizedName}.json"), "{}").ConfigureAwait(false);

                // We have for each formatting one file !
                // [DataTestMethod]
                // [DataRow("Json")]
                // [DataRow("JsonIndented")]
                // [DataRow("JsonAsString")]
                // Quickfix
                await File.WriteAllTextAsync(Path.Combine(outputFolder.FullName, $"{commandInfo.NormalizedName}AsJson.json"), "{}").ConfigureAwait(false);
                await File.WriteAllTextAsync(Path.Combine(outputFolder.FullName, $"{commandInfo.NormalizedName}AsJsonIndented.json"), "{}").ConfigureAwait(false);
                await File.WriteAllTextAsync(Path.Combine(outputFolder.FullName, $"{commandInfo.NormalizedName}AsJsonAsString.json"), "{}").ConfigureAwait(false);

                if (commandInfo.EndpointInfo.RequestType.IsNotNull())
                {
                    // Parameters folder
                    var parametersFolder = new DirectoryInfo(Path.Combine(targetFolder.FullName, "Parameters"));

                    if (parametersFolder.NotExists())
                    {
                        parametersFolder.Create();
                    }

                    await File.WriteAllTextAsync(Path.Combine(parametersFolder.FullName, $"{commandInfo.NormalizedName}.json"), "{}").ConfigureAwait(false);
                }
            }

            foreach (var commandInfoSubCommand in commandInfo.SubCommands)
            {
                await CreateTestsForCommandAsync(projectFileInfo, projectDocument, dotNetToolInfos,
                                                 commandInfoSubCommand, targetFolder, cliCallPath);
            }
        }
    }
}
