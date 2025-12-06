using System.Collections.Immutable;
using Argument.Check;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.Client;
using RunJit.Cli.Services;
using RunJit.Cli.Services.Endpoints;
using RunJit.Cli.Services.Resharper;
using Solution.Parser.CSharp;
using Solution.Parser.Solution;
using FileInfo = System.IO.FileInfo;

namespace RunJit.Cli.RunJit.Generate.Client
{
    // Api.Client
    // -> Api
    //    -> Projects
    //      -> V1
    //        -> Models
    //          -> GetAllProjectsResponse.cs
    //        -> ProjectV1.cs
    //    -> Users
    //      -> V1
    //        -> Models
    //          -> GetUserResponse.cs
    //        -> UserV1.cs
    //      -> V2
    //        -> Models
    //          -> GetUserResponse.cs
    //        -> UserV1.cs
    // -> Client.cs
    // -> ClientFactory.cs
    public record GeneratedClient(ImmutableList<GeneratedFacade> Facades,
                                  string SyntaxTree);

    public record GeneratedFacade(ImmutableList<GeneratedClientCodeForController> Endpoints,
                                  string SyntaxTree,
                                  string Domain,
                                  string FacadeName);

    public record GeneratedFacades(ImmutableList<GeneratedFacade> ControllerInfos);

    public record GeneratedClientCodeForController(EndpointGroup ControllerInfo,
                                                   string SyntaxTree,
                                                   string Domain);

    internal static class AddClientCreatorExtension
    {
        internal static void AddClientCreator(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddControllerParser();
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
            services.AddApiTypeLoader();
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

            services.AddSingletonIfNotExists<ClientCreator>();
        }
    }

    internal sealed class ClientCreator(ControllerParser controllerParser,
                                        ClientCreatorForController endpointClientGenerator,
                                        DomainFacedBuilder domainFacedBuilder,
                                        ClientBuilder clientBuilder,
                                        ClientFactoryBuilder clientFactoryBuilder,
                                        SolutionFileModifier solutionFileModifier,
                                        ResharperSettingsBuilder resharperSettingsBuilder,
                                        ClientStructureWriter clientStructureWriter,
                                        NugetUpdater nugetUpdater,
                                        ApiTypeLoader apiTypeLoader,
                                        TestStructureWriter testStructureWriter,
                                        RestructureController restructureController,
                                        MinimalApiEndpointParser minimalApiEndpointParser,
                                        OrganizeMinimalEndpoints organizeMinimalEndpoints,
                                        CurlBuilder curlBuilder,
                                        RequestPrinter requestPrinter,
                                        HttpCallHandler httpCallHandler,
                                        HttpCallHandlerFactory httpCallHandlerFactory,
                                        OpenApiJsonFileParser openApiJsonFileParser)
    {
        internal async Task GenerateClientAsync(Client client,
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
            var allSyntaxTrees = parsedSolution.ProductiveProjects.Where(p => !p.ProjectFileInfo.FileNameWithoutExtenion.EndsWith(clientProject.ProjectFileInfo.FileNameWithoutExtenion) && !p.ProjectFileInfo.FileNameWithoutExtenion.EndsWith(clientProject.ProjectFileInfo.FileNameWithoutExtenion + ".")).SelectMany(p => p.CSharpFileInfos.Select(c => c.Parse())).ToImmutableList();

            // 3. Get all types which are declared in the API assembly - Need to unique ident the types for client generation.
            var types = apiTypeLoader.GetAllTypesFrom(parsedSolution);

            if (client.UseOpenApiJson)
            {
                // Logic to get open api json
                var openApiJsonFile = new FileInfo(@"D:\Siemens\siemens-data-cloud-backend-console\src\Sdc.Console.Test\OpenApi\Responses\V1.json");
                var endpointsFromOpenApiJson = openApiJsonFileParser.ExtractFrom("api/console", allSyntaxTrees, types, openApiJsonFile);
                Console.WriteLine(endpointsFromOpenApiJson.Count);
            }

            // 4. Sync nuget packages - New feature we check which packages are predefined in the template
            //   and sync them up to the parent solution in which it will be included
            await nugetUpdater.UpdateAsync(parsedSolution, clientProject).ConfigureAwait(false);
            await nugetUpdater.UpdateAsync(parsedSolution, clientTestProject).ConfigureAwait(false);

            // 5. Get all controllers
            var controllerInfosOrg = controllerParser.ExtractFrom(allSyntaxTrees, types).OrderBy(controller => controller.Name).ToImmutableList();

            // 5.1 New to reorganize the controllers to get the correct domain name
            var controllerInfos = restructureController.Reorganize(controllerInfosOrg);

            // 5.2 Get all minimal endpoints
            var endpoints = minimalApiEndpointParser.ExtractFrom(allSyntaxTrees, types).OrderBy(controller => controller.Name).ToImmutableList();

            // 5.3 organize endpoints
            var organizedEndpoints = organizeMinimalEndpoints.Reorganize(endpoints);

            // 6. Get project name
            var projectName = clientProject.ProjectFileInfo.Value.NameWithoutExtension();

            // NEW !! TEST
            var mappedToEndpoints = organizedEndpoints.Any() ? organizedEndpoints : controllerInfos.ToEndpointInfos();

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

    internal static class ControllerInfosExtensions
    {
        internal static ImmutableList<EndpointGroup> ToEndpointInfos(this ImmutableList<ControllerInfo> controllerInfos)
        {
            var endpointGroups = ImmutableList.CreateBuilder<EndpointGroup>();

            foreach (var controllerInfo in controllerInfos)
            {
                var endpoints = ImmutableList.CreateBuilder<EndpointInfo>();

                foreach (var method in controllerInfo.Methods)
                {
                    var endpoint = method.ToEndpointInfo(controllerInfo.GroupName, controllerInfo.Version);
                    endpoints.Add(endpoint);
                }

                var endpointGroup = new EndpointGroup
                                    {
                                        GroupName = controllerInfo.GroupName,
                                        Endpoints = endpoints.ToImmutable(),
                                        Version = controllerInfo.Version
                                    };

                endpointGroups.Add(endpointGroup);
            }

            return endpointGroups.ToImmutable();
        }
    }

    internal static class MethodInfoExtensions
    {
        internal static EndpointInfo ToEndpointInfo(this MethodInfos methodInfo,
                                                    string groupName,
                                                    VersionInfo versionInfo)
        {
            var obsoleteValue = methodInfo.Attributes.FirstOrDefault(a => a.Name.StartsWith("Obsolete"))?.Arguments.FirstOrDefault();

            var endpoint = new EndpointInfo
                           {
                               ResponseType = methodInfo.ResponseType,
                               BaseUrl = methodInfo.RelativeUrl,
                               DomainName = methodInfo.Name,
                               HttpAction = methodInfo.HttpAction,
                               GroupName = groupName,
                               Parameters = methodInfo.Parameters,
                               SwaggerOperationId = methodInfo.SwaggerOperationId,
                               ProduceResponseTypes = methodInfo.ProduceResponseTypes,
                               RequestType = methodInfo.RequestType,
                               Version = versionInfo,
                               ObsoleteInfo = obsoleteValue.IsNull() ? null : new ObsoleteInfo(obsoleteValue),
                               Models = methodInfo.Models,
                               Name = methodInfo.Name,
                               RelativeUrl = methodInfo.RelativeUrl
                           };

            return endpoint;
        }
    }
}
