using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddHttpClientFactoryCodeGenExtension
    {
        internal static void AddHttpClientFactoryCodeGen(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddNamespaceProvider();

            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, HttpClientFactoryCodeGen>();
        }
    }

    internal sealed class HttpClientFactoryCodeGen(ConsoleService consoleService,
                                                   NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using System.Net.Http.Headers;
                                        using Extensions.Pack;
                                        using Microsoft.AspNetCore.Http;
                                        using Microsoft.Extensions.Configuration;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            internal static class Add$dotNetToolName$HttpClientFactoryExtension
                                            {
                                                internal static void Add$dotNetToolName$HttpClientFactory(this IServiceCollection services,
                                                                                                           IConfiguration configuration)
                                                {
                                                    if (services.IsAlreadyRegistered<$dotNetToolName$HttpClientFactory>())
                                                    {
                                                        return;
                                                    }
                                                
                                                    services.AddHttpClient();
                                                    services.Add$dotNetToolName$HttpClientSettings(configuration);
                                                    services.AddHttpCallHandlerFactory();

                                                    services.AddSingletonIfNotExists<$dotNetToolName$HttpClientFactory>();
                                                }
                                            }

                                            internal sealed class $dotNetToolName$HttpClientFactory(IHttpClientFactory httpClientFactory,
                                                                                                $dotNetToolName$HttpClientSettings aspNetCoreMinimalApiSdkClientSettings,
                                                                                                HttpCallHandlerFactory httpCallHandlerFactory)
                                            {
                                                private readonly string[] _allowedHeadersToCopy =
                                                [
                                                    "User-Agent",
                                                    "Authorization",
                                                    "x-",
                                                    "Referer"
                                                ];

                                                internal HttpCallHandler CreateFrom(HttpRequest httpRequest)
                                                {
                                                    var httpClient = httpClientFactory.CreateClient();
                                                    httpRequest.Headers.ForEach(headerEntry =>
                                                    {
                                                        // Important not content and host header copy
                                                        if (_allowedHeadersToCopy.Any(allowedHeader => headerEntry.Key.Contains(allowedHeader, StringComparison.OrdinalIgnoreCase)))
                                                        {
                                                            httpClient.DefaultRequestHeaders.Add(headerEntry.Key, headerEntry.Value.FirstOrDefault() ?? string.Empty);
                                                        }
                                                    });

                                                    httpClient.BaseAddress = new Uri(aspNetCoreMinimalApiSdkClientSettings.BaseAddress);

                                                    var httpClientHandler = httpCallHandlerFactory.CreateFrom(httpClient);

                                                    return httpClientHandler;
                                                }

                                                internal HttpCallHandler CreateFrom(string schema, string token)
                                                {
                                                    return CreateFrom($"{schema} {token}");
                                                }
                                                
                                                internal HttpCallHandler CreateFrom(string token)
                                                {
                                                    var httpClient = httpClientFactory.CreateClient();
                                                
                                                    // ToDo: We are in a cli we can just setup http client once as singleton
                                                    //       We need this also for testing
                                                    if (httpClient.BaseAddress.IsNull())
                                                    {
                                                        httpClient.BaseAddress = new Uri(aspNetCoreMinimalApiSdkClientSettings.BaseAddress);
                                                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                        httpClient.DefaultRequestHeaders.Add("User-Agent", "DotNetToolName");
                                                    }
                                                    
                                                    var httpClientHandler = httpCallHandlerFactory.CreateFrom(httpClient);
                                                    return httpClientHandler;
                                                }
                                            }

                                            internal static class Add$dotNetToolName$HttpClientSettingsExtension
                                            {
                                                internal static void Add$dotNetToolName$HttpClientSettings(this IServiceCollection services,
                                                    IConfiguration configuration)
                                                {
                                                    if (configuration.TryGetSettings<$dotNetToolName$HttpClientSettings>(out var clientSettings).IsFalse())
                                                    {
                                                        clientSettings = new $dotNetToolName$HttpClientSettings();
                                                    }

                                                    services.AddSingletonIfNotExists(clientSettings);
                                                }
                                            }

                                            internal sealed record $dotNetToolName$HttpClientSettings
                                            {
                                                public string BaseAddress { get; init; } = "http://staging/api/$dotNetToolName$/";
                                            }
                                        }

                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add HttpCallHandler Folder
            var appFolder = new DirectoryInfo(Path.Combine(projectFileInfo.Directory!.FullName, "HttpCallHandlers"));

            if (appFolder.NotExists())
            {
                appFolder.Create();
            }

            // 2. Add HttpCallHandler.cs
            var file = Path.Combine(appFolder.FullName, $"{dotNetToolInfos.NormalizedName}HttpClientFactory.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName);

            var formattedTemplate = newTemplate.FormatSyntaxTree();

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 3. Adjust namespace provider
            namespaceProvider.SetNamespaceProvider(projectFileInfo, $"{dotNetToolInfos.ProjectName}.HttpCallHandlers", true);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
