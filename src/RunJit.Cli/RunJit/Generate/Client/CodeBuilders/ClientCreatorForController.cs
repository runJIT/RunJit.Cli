﻿using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Generate.Client
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
    internal class ClientCreatorForController(MethodBuilder methodBuilder)
    {
        private readonly string _versionClass = EmbeddedFile.GetFileContentFrom("RunJit.Generate.Client.Templates.version.class.rps");


        internal IImmutableList<GeneratedClientCodeForController> Create(IImmutableList<ControllerInfo> controllerInfos, string projectName, string clientName)
        {
            return controllerInfos.Select(controller => Create(controller, projectName, clientName)).ToImmutableList();
        }


        internal GeneratedClientCodeForController Create(ControllerInfo controllerInfo, string projectName, string clientName)
        {
            var domainName = $"{controllerInfo.Name.Replace("Controller", string.Empty)}";
            var domainNameWithVersion = $"{domainName}{controllerInfo.Version.Normalized}";

            var methods = methodBuilder.BuildFor(controllerInfo);

            var @namespace = $"{projectName}.{ClientGenConstants.Api}.{domainName}.{controllerInfo.Version.Normalized}";

            var controllerObsoleteAttribute = controllerInfo.Attributes.FirstOrDefault(a => a.Name == "Obsolete");
            var attributes = controllerObsoleteAttribute.IsNotNull() ? controllerObsoleteAttribute.SyntaxTree : string.Empty;

            var methodSyntax = methods.Flatten($"{Environment.NewLine}{Environment.NewLine}");


            var syntaxTree = _versionClass.Replace("$name$", domainName)
                                          .Replace("$version$", controllerInfo.Version.Normalized)
                                          .Replace("$methods$", methodSyntax)
                                          .Replace("$projectName$", projectName)
                                          .Replace("$clientName$", clientName)
                                          .Replace("$namespace$", @namespace)
                                          .Replace("$attributes$", attributes);

            return new GeneratedClientCodeForController(controllerInfo, syntaxTree, domainNameWithVersion);
        }
    }
}
