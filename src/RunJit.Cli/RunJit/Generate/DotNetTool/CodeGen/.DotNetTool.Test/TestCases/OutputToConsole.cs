using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool.Models;

namespace RunJit.Cli.Generate.DotNetTool.DotNetTool.Test
{
    internal static class AddOutputToConsoleCodeGenExtension
    {
        internal static void AddOutputToConsoleCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolTestCaseCodeGen, OutputToConsoleCodeGen>();
        }
    }

    internal sealed class OutputToConsoleCodeGen : IDotNetToolTestCaseCodeGen
    {
        private const string Template = """
                                                /// <summary>
                                                /// Tests the '$callPathSummary$' command with different output formats and writes the output to a specified file.
                                                /// Asserts the output against the expected JSON file.
                                                /// </summary>
                                                /// <param name="format">The format in which the configuration should be output (e.g., Json, JsonIndented, JsonAsString).</param>
                                                /// <returns>A task representing the asynchronous operation.</returns>
                                                [DataTestMethod]
                                                [DataRow("Json")]
                                                [DataRow("JsonIndented")]
                                                [DataRow("JsonAsString")]
                                                public Task $testMethodName$(string format)
                                                {
                                                    // 1. Executes the CLI command and asserts the output against the expected JSON file.
                                                    return Cli.AssertRunAsync($"$cliCall$",
                                                                              $"$expectedOutput$");
                                                }
                                        """;

        public Task<string> GenerateAsync(DotNetToolInfos dotNetToolInfos,
                                          CommandInfo commandInfo,
                                          string cliCallPath)
        {
            if (commandInfo.EndpointInfo.IsNull())
            {
                return Task.FromResult(string.Empty);
            }

            var expectedOutput = cliCallPath.Split(" ").Where(value => value.StartsWith("-").IsFalse()).Flatten(".");
            expectedOutput = $"{commandInfo.NormalizedName}As{{format}}.json";

            var testMethodName = cliCallPath.Split(" ").Where(value => value.StartsWith("-").IsFalse()).Flatten("_");
            var parametersFile = cliCallPath.Split(" ").Where(value => value.StartsWith("-").IsFalse()).Flatten(".");

            //parametersFile = $"{parametersFile}.Parameters.{commandInfo.NormalizedName}.json";

            // Brand new test sdk have now an embedded file locator, name only is enough :)
            parametersFile = $"{commandInfo.NormalizedName}As{{format}}.json";

            var cliCall = cliCallPath;

            var callWithArgs = commandInfo.EndpointInfo.IsNotNull() &&
                               (commandInfo.EndpointInfo.Parameters.Any() ||
                                commandInfo.EndpointInfo.RequestType.IsNotNull());

            var cliCallWithArgument = callWithArgs ? $"{cliCall.ToLowerInvariant()} {parametersFile}" : $"{cliCall.ToLowerInvariant()}";

            var cliCallWithArgumentAndOutput = $"{cliCallWithArgument} --format {{format}}";

            var commandAsFolderPath = cliCallPath.Replace(" ", ",");

            var newTemplate = Template.Replace("$namespace$", $"{dotNetToolInfos.ProjectName}.Test")
                                      .Replace("$cliCall$", cliCallWithArgumentAndOutput)
                                      .Replace("$testMethodName$", $"{testMethodName}_To_Console_Out_With_Format")
                                      .Replace("$expectedOutput$", expectedOutput)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName.ToLower())
                                      .Replace("$commandName$", commandInfo.NormalizedName)
                                      .Replace("$parametersFile$", parametersFile)
                                      .Replace("$commandAsFolderPath$", commandAsFolderPath)
                                      .Replace("$callPathSummary$", cliCallPath.ToLower());

            return Task.FromResult(newTemplate);
        }
    }
}
