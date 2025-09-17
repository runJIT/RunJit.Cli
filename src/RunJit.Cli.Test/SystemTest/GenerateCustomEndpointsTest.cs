using System.Collections.Immutable;
using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.RunJit.Generate.CustomEndpoint;
using RunJit.Cli.Test.Commands;
using RunJit.Cli.Test.Extensions;
using Solution.Parser.Solution;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("pulse generate custom-endpoint")]
    [TestClass]
    public class GenerateCustomEndpointsTest : GlobalSetup
    {
        private const string BasePath = "api/users";

        [TestMethod]
        [DataRow(@"D:\AzureDevOps\SoftwareOne.Workshop.November.2023\RunJit\Templates\AddUser.json")]
        public async Task Generate_Custom_Endpoints(string template)
        {
            // 0. Project name
            var projectName = "UserManagement";

            // 1. Create new Web Api
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi(projectName, WebApiFolder, BasePath)).ConfigureAwait(false);

            // 2. Get web api project
            var parsedSolution = new SolutionFileInfo(solutionFile.FullName).Parse();
            var webAppProject = parsedSolution.ProductiveProjects.First(p => p.Document.ToString().Contains("Sdk=\"Microsoft.NET.Sdk.Web\""));

            // 2. Generate rest api from a sql 
            var endpointData = (await File.ReadAllTextAsync(template).ConfigureAwait(false)).FromJsonStringAs<EndpointData>();

            var json = endpointData.ToJsonIntended();

            // iterate over the endpointData recursively.
            // for each file content if it is a path, read the file and replace the content wih the file content
            await WalkThroughTree(endpointData.Templates).ConfigureAwait(false);

            await Mediator.SendAsync(new GenerateCustomEndpoint(webAppProject.ProjectFileInfo.Value.Directory!, endpointData, true)).ConfigureAwait(false);

            // 3. Test if generated solution can be build :) 
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);
        }

        private static async Task WalkThroughTree(IImmutableList<Template> templates)
        {
            foreach (var endpointDataTemplate in templates)
            {
                foreach (var codeFile in endpointDataTemplate.Files)
                {
                    if (Path.IsPathFullyQualified(codeFile.Content))
                    {
                        var fileContentAsFileInfo = new FileInfo(codeFile.Content);
                        codeFile.Content = await File.ReadAllTextAsync(fileContentAsFileInfo.FullName).ConfigureAwait(false);
                    }
                }

                await WalkThroughTree(endpointDataTemplate.Templates).ConfigureAwait(false);
            }
        }
    }

    internal sealed record GenerateCustomEndpoint(DirectoryInfo DirectoryInfo,
                                                  EndpointData EndpointData,
                                                  bool OverwriteCode) : ICommand;

    internal sealed class GenerateCustomEndpointHandler : ICommandHandler<GenerateCustomEndpoint>
    {
        public async Task Handle(GenerateCustomEndpoint request,
                                 CancellationToken cancellationToken)
        {
            await using var sw = new StringWriter();
            Console.SetOut(sw);

            var strings = CollectConsoleParameters(request).ToArray();
            var consoleCall = strings.Flatten(" ");
            Console.WriteLine();
            Console.WriteLine(consoleCall);
            Debug.WriteLine(consoleCall);
            var exitCode = await Program.Main(strings).ConfigureAwait(false);
            var output = sw.ToString();

            Debug.WriteLine(output);

            Assert.AreEqual(0, exitCode, output);
        }

        private IEnumerable<string> CollectConsoleParameters(GenerateCustomEndpoint request)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "generate";
            yield return "custom-endpoint";

            yield return "--target-folder";
            yield return request.DirectoryInfo.FullName;

            yield return "--endpoint-data";

            var json = request.EndpointData.ToJsonIntended();

            yield return json;

            yield return "--overwrite-code";
            yield return request.OverwriteCode.ToString();
        }
    }
}
