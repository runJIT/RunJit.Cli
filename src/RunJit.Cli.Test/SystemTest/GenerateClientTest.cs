using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Commands;
using RunJit.Cli.Test.Extensions;

namespace RunJit.Cli.Test.SystemTest
{
    [TestCategory("pulse generate client")]
    [TestClass]
    public class GenerateClientTest : GlobalSetup
    {
        private const string Resource = "User";

        private const string BasePath = "api/client-gen";

        [TestMethod]
        public async Task Generate_Client_For_Simple_Rest_Endpoint()
        {
            // 1. Create new Web Api
            var solutionFile = await Mediator.SendAsync(new CreateNewSimpleWebApi("Runjit.ClientGen", WebApiFolder, BasePath)).ConfigureAwait(false);

            // 2. Create Web-Api endpoints
            await Mediator.SendAsync(new CreateSimpleRestController(solutionFile, Resource, false)).ConfigureAwait(false);

            // 3. Generate client for endpoint
            await Mediator.SendAsync(new GenerateClient(solutionFile)).ConfigureAwait(false);

            //// 4. Test if generated results is buildable
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFile.FullName}").ConfigureAwait(false);
        }

        //[Ignore("Dev only")]
        [TestMethod]
        [DataRow(@"D:\AzureDevOps\AspNetCore.MinimalApi.Sdk\AspNetCore.MinimalApi.Sdk.sln")]
        [DataRow(@"D:\GitHub\RunJit.Api\RunJit.Api.sln")]
        [DataRow(@"D:\Siemens\pulse-core\PulseCore.sln")]
        [DataRow(@"D:\Siemens\pulse-flow\Pulse.Flow.sln")]
        [DataRow(@"D:\AzureDevOps\SoftwareOne.Workshop.November.2023\RunJit\UserManagement\UserManagement.sln")]
        [DataRow(@"D:\Siemens\pulse-sustainability\Pulse.Sustainability.sln")]
        [DataRow("/Users/z003m9sc/Documents/RiderProjects/SiemensGPT/siemensgpt-backend/SiemensGPT.sln")]
        [DataRow("/Users/z003m9sc/Documents/RiderProjects/PulseCloud/pulse-nexus/Pulse.Nexus.sln")]
        [DataRow(@"D:\Siemens\pulse-fieldingtool\Pulse.FieldingTool.sln")]
        [DataRow(@"D:\Siemens\siemens-data-cloud-backend-core\Sdc.Core.sln")]
        [DataRow(@"D:\Siemens\siemens-data-cloud-backend-console\Sdc.Console.sln")]
        public Task Generate_Client_Of_Existing_Solution_For(string solutionPath)
        {
            return Mediator.SendAsync(new GenerateClient(new FileInfo(solutionPath), false));
        }
        
        [Ignore("Dev only")]
        [TestMethod]
        public async Task Next_Level_Parsing()
        {
            // Adjust the path to your solution file
            var solutionPath = "D:\\AzureDevOps\\AspNetCore.MinimalApi.Sdk\\AspNetCore.MinimalApi.Sdk.sln";

            // Load the workspace and project
            var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(solutionPath).ConfigureAwait(false);

            var project = solution.Projects.FirstOrDefault(p => p.Name == "MinimalApi");

            if (project == null)
            {
                Console.WriteLine("Project not found.");

                return;
            }

            // Get the compilation
            var compilation = await project.GetCompilationAsync().ConfigureAwait(false);

            // Find the document and the syntax tree
            var document = project.Documents.FirstOrDefault(d => d.Name == "GetAllToDoEndpoints.cs");
            Assert.IsNotNull(document);

            var syntaxTree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);

            Assert.IsNotNull(syntaxTree);

            // Get the semantic model
            Assert.IsNotNull(compilation);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var root = syntaxTree.GetRoot();

            var returnStatement = root.DescendantNodes()
                                      .OfType<ReturnStatementSyntax>()
                                      .Last();

            var returnExpression = returnStatement.Expression as InvocationExpressionSyntax;

            Assert.IsNotNull(returnExpression);

            var methodSymbol = semanticModel.GetSymbolInfo(returnExpression).Symbol as IMethodSymbol;
            var returnType = methodSymbol?.ReturnType;

            Console.WriteLine($"Return type: {returnType}");
        }
    }

    internal sealed record GenerateClient(FileInfo SolutionFile,
                                          bool BuildBeforeGenerate = true) : ICommand;

    internal sealed class GenerateClientHandler : ICommandHandler<GenerateClient>
    {
        public async Task Handle(GenerateClient request,
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

            Assert.AreEqual(0, exitCode, output);
        }

        private IEnumerable<string> CollectConsoleParameters(GenerateClient request)
        {
            // 1. Parameter solution file from the backend to parse
            yield return "runjit";
            yield return "generate";
            yield return "client";
            yield return "--solution";
            yield return request.SolutionFile.FullName;

            if (request.BuildBeforeGenerate)
            {
                yield return "--build";
            }
        }
    }
}
