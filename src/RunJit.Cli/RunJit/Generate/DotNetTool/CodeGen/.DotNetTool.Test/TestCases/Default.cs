using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool.Models;

namespace RunJit.Cli.Generate.DotNetTool.DotNetTool.Test
{
    internal static class AddDefaultTestCodeGenExtension
    {
        internal static void AddDefaultTestCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolTestCaseCodeGen, DefaultTestCodeGen>();
        }
    }

    internal sealed class DefaultTestCodeGen : IDotNetToolTestCaseCodeGen
    {
        private const string Template = """
                                                /// <summary>
                                                /// Tests the '$callPathSummary$' command with different output formats and writes the output to a specified file.
                                                /// Asserts the output against the expected JSON file.
                                                /// </summary>
                                                /// <returns>A task representing the asynchronous operation.</returns>
                                                [TestMethod]
                                                public Task $testMethodName$()
                                                {
                                                    // 1. Executes the CLI command and asserts the output against the expected JSON file.
                                                    return Cli.AssertRunAsync($"$cliCall$",
                                                                              "$expectedOutput$");
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

            // Brand new test sdk have now an embedded file locator, name only is enough :)
            expectedOutput = $"{commandInfo.NormalizedName}.json";

            var testMethodName = cliCallPath.Split(" ").Where(value => value.StartsWith("-").IsFalse()).Flatten("_");
            var parametersFile = cliCallPath.Split(" ").Where(value => value.StartsWith("-").IsFalse()).Flatten(".");

            // Brand new test sdk have now an embedded file locator, name only is enough :)
            parametersFile = $"{commandInfo.NormalizedName}.json";

            var cliCall = cliCallPath;

            var callWithArgs = commandInfo.EndpointInfo.IsNotNull() &&
                               (commandInfo.EndpointInfo.Parameters.Any() ||
                                commandInfo.EndpointInfo.RequestType.IsNotNull());

            var cliCallWithArgument = callWithArgs ? $"{cliCall.ToLowerInvariant()} {parametersFile}" : $"{cliCall.ToLowerInvariant()}";

            var cliCallWithArgumentAndOutput = $"{cliCallWithArgument}";

            var commandAsFolderPath = cliCallPath.Split(" ").Select(value => $"\"{value}\"").Flatten(", ");

            var newTemplate = Template.Replace("$namespace$", $"{dotNetToolInfos.ProjectName}.Test")
                                      .Replace("$cliCall$", cliCallWithArgumentAndOutput)
                                      .Replace("$testMethodName$", $"{testMethodName}")
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
