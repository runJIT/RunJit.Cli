using System.Collections.Immutable;
using Argument.Check;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;
using RunJit.Cli.Services;
using RunJit.Cli.Services.Resharper;
using Solution.Parser.CSharp;
using Solution.Parser.Solution;

namespace RunJit.Cli.Generate.Client
{
    internal static class AddOpenApiJsonClientGeneratorExtension
    {
        internal static void AddOpenApiJsonClientGenerator(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddClientCreatorForController();
            services.AddDomainFacedBuilder();
            services.AddClientBuilder();
            services.AddClientFactoryBuilder();
            services.AddModelBuilder();
            services.AddSolutionFileModifier();
            services.AddResharperSettingsBuilder();
            services.AddClientStructureWriter();
            services.AddNugetUpdater();
            services.AddAssemblyTypeLoader();
            services.AddTestStructureWriter();
            services.AddRestructureController();

            services.AddMinimalApiEndpointParser();
            services.AddOrganizeMinimalEndpoints();

            services.AddSolutionCodeCleanup();
            services.AddCurlBuilder();
            services.AddRequestPrinter();

            services.AddHttpCallHandler();
            services.AddHttpCallHandlerFactory();

            services.AddJsonSerializerBuilder();
            services.AddOpenApiJsonFileParser();

            services.AddSingletonIfNotExists<ISpecificClientGenerator, OpenApiJsonClientGenerator>();
        }
    }

    internal class OpenApiJsonClientGenerator(ClientCreatorForController endpointClientGenerator,
                                              DomainFacedBuilder domainFacedBuilder,
                                              ClientBuilder clientBuilder,
                                              ClientFactoryBuilder clientFactoryBuilder,
                                              SolutionFileModifier solutionFileModifier,
                                              ResharperSettingsBuilder resharperSettingsBuilder,
                                              ClientStructureWriter clientStructureWriter,
                                              TestStructureWriter testStructureWriter,
                                              OrganizeMinimalEndpoints organizeMinimalEndpoints,
                                              CurlBuilder curlBuilder,
                                              RequestPrinter requestPrinter,
                                              HttpCallHandler httpCallHandler,
                                              HttpCallHandlerFactory httpCallHandlerFactory,
                                              OpenApiJsonFileParser openApiJsonFileParser) : ISpecificClientGenerator
    {
        public bool CanHandle(RunJit.Generate.Client.Client client)
        {
            return client.UseOpenApiJson;
        }

        public async Task GenerateClientAsync(RunJit.Generate.Client.Client client,
                                              FileInfo clientSolution)
        {
            // 0. Add new client projects into existing solution
            await solutionFileModifier.AddProjectsAsync(client.SolutionFileInfo).ConfigureAwait(false);

            // 1. Parse source solution - Meta level no code parsing here
            var parsedSolution = new SolutionFileInfo(client.SolutionFileInfo.FullName).Parse();
            var parsedClientSolution = new SolutionFileInfo(clientSolution.FullName).Parse();
            var clientProjectName = $"{parsedClientSolution.SolutionFileInfo.FileNameWithoutExtenion}.Client";
            var clientProject = parsedClientSolution.ProductiveProjects.First(p => p.ProjectFileInfo.FileNameWithoutExtenion.EndsWith(clientProjectName));
            var clientTestProject = parsedClientSolution.UnitTestProjects.First(p => p.ProjectFileInfo.FileNameWithoutExtenion.EndsWith($"{clientProjectName}.Test"));
            var clientName = clientProject.ProjectFileInfo.FileNameWithoutExtenion.Replace(".", string.Empty);

            // 2. Parse all C# files
            // var allSyntaxTrees = parsedSolution.ProductiveProjects.Where(p => !p.ProjectFileInfo.FileNameWithoutExtenion.EndsWith(clientProject.ProjectFileInfo.FileNameWithoutExtenion) && !p.ProjectFileInfo.FileNameWithoutExtenion.EndsWith(clientProject.ProjectFileInfo.FileNameWithoutExtenion + ".")).SelectMany(p => p.CSharpFileInfos.Select(c => c.Parse())).ToImmutableList();

            // 6. Get project name
            var projectName = clientProject.ProjectFileInfo.Value.NameWithoutExtension();

            // ToDo: To avoid duplicated models, request response types
            //       we should create a model folder and write them all in, make the namespace neutral
            //       like in swagger the component part
            // Logic to get open api json
            var openApiJsonFile = new FileInfo(@"D:\Siemens\siemens-data-cloud-backend-console\src\Sdc.Console.Test\OpenApi\Responses\V1.json");

            var endpointsFromOpenApiJson = openApiJsonFileParser.ExtractFrom("api/console", clientName, openApiJsonFile);

            Console.WriteLine(endpointsFromOpenApiJson.Count);

            // 5.3 organize endpoints
            var mappedToEndpoints = organizeMinimalEndpoints.Reorganize(endpointsFromOpenApiJson);


            // 7. Collect all endpoints
            var allEndpoints = endpointClientGenerator.Create(mappedToEndpoints, projectName, clientName);

            // NEW Default health endpoint

            // 8. Create facades for each domain endpoint
            var facades = domainFacedBuilder.BuildFrom(allEndpoints, projectName, clientName);

            // 9. Create client for facades
            var generatedClient = clientBuilder.BuildFor(facades, projectName, clientName);

            // 10. Create client factory for client, specific for siemens usage with HttpRequest's
            var clientFactory = clientFactoryBuilder.BuildFor(projectName, clientName, generatedClient);

            // 11. Create curl builder
            var curlBuilderCode = curlBuilder.BuildFor(projectName, clientName);

            // 12. Create Request printer
            var requestPrinterCode = requestPrinter.BuildFor(projectName, clientName);

            // 13. Setup R# namespace providers.
            var resharperSettings = resharperSettingsBuilder.BuildFrom(generatedClient);

            // 14. Get client folder
            var clientFolder = clientProject.ProjectFileInfo.Value.Directory!;

            // 15. Create Curl folder
            var curlFolder = new DirectoryInfo(Path.Combine(clientFolder.FullName, "Curl"));

            if (curlFolder.NotExists())
            {
                curlFolder.Create();
            }

            // 16. Now we overwrite the HttpCallHandler
            var httpCallHandlerCode = httpCallHandler.BuildFor(projectName, clientName);

            // 17. Now we overwrite the HttpCallHandlerFactory
            var httpCallHandlerFactoryCode = httpCallHandlerFactory.BuildFor(projectName, clientName);

            // 18. HttpCallHandlers folder have to exist
            var httpCallHandlersFolder = new DirectoryInfo(Path.Combine(clientFolder.FullName, "HttpCallHandlers"));
            Throw.IfNotExists(httpCallHandlersFolder);

            // 19. Write client class
            await File.WriteAllTextAsync(Path.Combine(clientFolder.FullName, $"{clientName}.cs"), generatedClient.SyntaxTree).ConfigureAwait(false);

            // 20. Write client factory class
            await File.WriteAllTextAsync(Path.Combine(clientFolder.FullName, $"{clientName}Factory.cs"), clientFactory).ConfigureAwait(false);

            // 21. Write R# settings
            await File.WriteAllTextAsync($"{clientProject.ProjectFileInfo.Value.FullName}.DotSettings", resharperSettings).ConfigureAwait(false);

            // 22. Write new CurlBuilder
            await File.WriteAllTextAsync(Path.Combine(curlFolder.FullName, "CurlBuilder.cs"), curlBuilderCode).ConfigureAwait(false);

            // 23. Write new Request printer
            await File.WriteAllTextAsync(Path.Combine(curlFolder.FullName, "RequestPrinter.cs"), requestPrinterCode).ConfigureAwait(false);

            // 24. Update http call handler
            await File.WriteAllTextAsync(Path.Combine(httpCallHandlersFolder.FullName, "HttpCallHandler.cs"), httpCallHandlerCode).ConfigureAwait(false);

            // 25. Update http call handler factory
            await File.WriteAllTextAsync(Path.Combine(httpCallHandlersFolder.FullName, "HttpCallHandlerFactory.cs"), httpCallHandlerFactoryCode).ConfigureAwait(false);

            // 26. Write facades, domain and version classes
            await clientStructureWriter.WriteFileStructureAsync(generatedClient, clientProject, projectName,
                                                                clientName).ConfigureAwait(false);

            // 19.Creates or updates base test structure
            await testStructureWriter.WriteFileStructureAsync(parsedSolution, clientProject, clientTestProject,
                                                              projectName, clientName).ConfigureAwait(false);
        }
    }
}
