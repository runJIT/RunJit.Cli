using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;
using RunJit.Cli.Services.Endpoints;

namespace RunJit.Cli.Generate.Client
{
    internal static class AddClientCreatorForControllerExtension
    {
        internal static void AddClientCreatorForController(this IServiceCollection services)
        {
            services.AddMethodBuilder();

            services.AddSingletonIfNotExists<ClientCreatorForController>();
        }
    }

    // What we are creating here:
    // - We are creating a specific client class per controller in the api
    // - The code template RunJit.Generate.Client.Templates.version.class.rps contains most predefined stuff which have to be replaced only
    //
    // Sample:
    //
    // internal static class AddAccountsV1Extension
    // {
    //     internal static void AddAccountsV1(this IServiceCollection services)
    //     {
    //         services.AddHttpCallHandler();
    //     }
    // }

    // public class AccountsV1  <-- DomainName -> Controller name shorten without controller postfix
    // {
    //     private readonly IHttpCallHandler _httpCallHandler;

    //     public AccountsV1(IHttpCallHandler httpCallHandler)
    //     {
    //         _httpCallHandler = httpCallHandler;
    //     }

    //     public Task<AddAccountResponse> AddAccountAsync(AddAccountPayload addAccountPayload)  <-- MethodBuilder creates all methods
    //     {
    //         return _httpCallHandler.CallAsync<AddAccountResponse>(HttpMethod.Post, $"v1.0/accounts", addAccountPayload);
    //     }
    // }
    internal sealed class ClientCreatorForController(MethodBuilder methodBuilder)
    {
        private readonly string _versionClass = EmbeddedFile.GetFileContentFrom("RunJit.Generate.Client.Templates.version.class.rps");

        internal ImmutableList<GeneratedClientCodeForController> Create(ImmutableList<EndpointGroup> endpointGroups,
                                                                        string projectName,
                                                                        string clientName)
        {
            return endpointGroups.Select(controller => Create(controller, projectName, clientName)).ToImmutableList();
        }

        internal GeneratedClientCodeForController Create(EndpointGroup endpointGroup,
                                                         string projectName,
                                                         string clientName)
        {
            var domainName = endpointGroup.GroupName;
            var domainNameWithVersion = endpointGroup.Version.IsNull() ? domainName : $"{domainName}{endpointGroup.Version.Normalized}";

            var methods = methodBuilder.BuildFor(endpointGroup);

            var @namespace = endpointGroup.Version.IsNotNull() ? $"{projectName}.{ClientGenConstants.Api}.{domainName}.{endpointGroup.Version.Normalized}" : $"{projectName}.{ClientGenConstants.Api}.{domainName}";

            //var controllerObsoleteAttribute = endpointGroup.Attributes.FirstOrDefault(a => a.Name == "Obsolete");
            //var attributes = controllerObsoleteAttribute.IsNotNull() ? controllerObsoleteAttribute.SyntaxTree : string.Empty;
            var attributes = string.Empty;

            var methodSyntax = methods.Flatten($"{Environment.NewLine}{Environment.NewLine}");

            var syntaxTree = _versionClass.Replace("$name$", domainName)
                                          .Replace("$version$", endpointGroup.Version?.Normalized ?? string.Empty)
                                          .Replace("$methods$", methodSyntax)
                                          .Replace("$projectName$", projectName)
                                          .Replace("$clientName$", clientName)
                                          .Replace("$namespace$", @namespace)
                                          .Replace("$attributes$", attributes);

            return new GeneratedClientCodeForController(endpointGroup, syntaxTree, domainNameWithVersion);
        }
    }
}
