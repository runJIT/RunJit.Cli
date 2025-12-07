using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.Client;
using RunJit.Cli.Services.Endpoints;
using ControllerInfo = RunJit.Cli.Services.Endpoints.ControllerInfo;
using MethodInfos = RunJit.Cli.Services.Endpoints.MethodInfos;
using VersionInfo = RunJit.Cli.Services.Endpoints.VersionInfo;

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
            services.AddSolutionClientGenerator();
            services.AddOpenApiJsonClientGenerator();

            services.AddSingletonIfNotExists<ClientCreator>();
        }
    }

    internal sealed class ClientCreator(IEnumerable<ISpecificClientGenerator> clientGenerators)
    {
        internal async Task GenerateClientAsync(Client client,
                                                FileInfo clientSolution)
        {

            var canHandler = clientGenerators.Where(g => g.CanHandle(client)).ToImmutableList();
            if (canHandler.Count <= 0)
            {
                throw new InvalidOperationException("No strategy was found to handle the client gen request");
            }

            if (canHandler.Count > 1)
            {
                throw new InvalidOperationException("Multiple strategies were found to handle the client gen request");
            }

            await canHandler[0].GenerateClientAsync(client, clientSolution).ConfigureAwait(false);
        }
    }


    interface ISpecificClientGenerator
    {
        bool CanHandle(Client client);
        Task GenerateClientAsync(Client client,
                                 FileInfo clientSolution);
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
