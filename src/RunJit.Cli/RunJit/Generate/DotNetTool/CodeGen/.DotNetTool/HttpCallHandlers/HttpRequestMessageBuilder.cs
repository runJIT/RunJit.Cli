using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddHttpRequestMessageBuilderCodeGenExtension
    {
        internal static void AddHttpRequestMessageBuilderCodeGen(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddNamespaceProvider();

            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, HttpRequestMessageBuilderCodeGen>();
        }
    }

    internal sealed class HttpRequestMessageBuilderCodeGen(ConsoleService consoleService,
                                                           NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using System.Net;
                                        using System.Net.Mime;
                                        using System.Text;
                                        using System.Text.Json;
                                        using Extensions.Pack;
                                        using Microsoft.Extensions.Configuration;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            internal static class AddHttpRequestMessageBuilderExtension
                                            {
                                                internal static void AddHttpRequestMessageBuilder(this IServiceCollection services)
                                                {
                                                    services.AddSingletonIfNotExists<HttpRequestMessageBuilder>();
                                                }
                                            }

                                            internal sealed class HttpRequestMessageBuilder
                                            {
                                                internal HttpRequestMessage BuildFrom(HttpMethod method,
                                                                                      string uri,
                                                                                      object? payload,
                                                                                      string payloadParameterName)
                                                {
                                                    var content = payload.IsNotNull() ? GetContent(payload, payloadParameterName) : null;
                                                    return new HttpRequestMessage(method, uri) { Version = HttpVersion.Version11, VersionPolicy = HttpVersionPolicy.RequestVersionOrLower, Content = content };

                                                    static HttpContent GetContent(object? payload, string payloadParameterName)
                                                    {
                                                        return payload switch
                                                        {
                                                            string stringContent => new StringContent(stringContent),
                                                            InMemoryFileAsStream inMemoryFileAsStream => inMemoryFileAsStream.ToMultipartFormDataContent(payloadParameterName),
                                                            byte[] byteArray => new ByteArrayContent(byteArray),
                                                            Stream streamContent => new StreamContent(streamContent),
                                                            _ => new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, MediaTypeNames.Application.Json), // Default any class or record converted to json string
                                                        };
                                                    }
                                                }
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
            var file = Path.Combine(appFolder.FullName, "HttpRequestMessageBuilder.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName);

            var formattedTemplate = newTemplate.FormatSyntaxTree();

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 3. Adjust namespace provider
            namespaceProvider.SetNamespaceProvider(projectFileInfo, $"{dotNetToolInfos.ProjectName}.HttpRequestMessageBuilder", true);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
