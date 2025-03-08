using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddHttpCallHandlerCodeGenExtension
    {
        internal static void AddHttpCallHandlerCodeGen(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddNamespaceProvider();

            services.AddHttpClient();

            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, HttpCallHandlerCodeGen>();
        }
    }

    internal sealed class HttpCallHandlerCodeGen(ConsoleService consoleService,
                                                 NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using System.Runtime.CompilerServices;

                                        namespace $namespace$
                                        {
                                            internal sealed class HttpCallHandler(HttpClient httpClient,
                                                                                  ResponseTypeHandleStrategy responseHandleStrategy,
                                                                                  HttpRequestMessageBuilder httpRequestMessageBuilder)
                                            {
                                                internal async Task<string> CallAsync(HttpMethod httpMethod,
                                                                                      string url,
                                                                                      object? payload,
                                                                                      CancellationToken cancellationToken,
                                                                                      [CallerArgumentExpression(nameof(payload))] string payloadParameterName = "")
                                                {
                                                    var message = httpRequestMessageBuilder.BuildFrom(httpMethod, url, payload, payloadParameterName);
                                                    var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
                                                    var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                                        
                                                    if (response.IsSuccessStatusCode)
                                                    {
                                                        return content;
                                                    }
                                        
                                                    var absoluteUrl = $"{httpClient.BaseAddress}{url}";
                                        
                                                    throw new ProblemDetailsException("Client call to endpoint was not successful",
                                                                                      $"The http call: {httpMethod.Method} {url} was not successful",
                                                                                      ("HttpMethod", httpMethod.Method),
                                                                                      ("Url", absoluteUrl),
                                                                                      ("Error", content));
                                                }
                                        
                                                internal async Task<TResult> CallAsync<TResult>(HttpMethod httpMethod,
                                                                                                string url,
                                                                                                object? payload,
                                                                                                CancellationToken cancellationToken,
                                                                                                [CallerArgumentExpression(nameof(payload))] string payloadParameterName = "")
                                                {
                                                    var message = httpRequestMessageBuilder.BuildFrom(httpMethod, url, payload, payloadParameterName);
                                                    var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
                                                    var result = await responseHandleStrategy.HandleAsync<TResult>(response, httpMethod, httpClient, url).ConfigureAwait(false);
                                                    return result;
                                                }
                                        
                                        
                                                internal async Task CallAsync(Func<HttpClient, string, string, Task<HttpResponseMessage>> httpFunction,
                                                                              HttpMethod httpMethod,
                                                                              string url,
                                                                              string paylod)
                                                {
                                                    var response = await httpFunction(httpClient, url, paylod).ConfigureAwait(false);
                                                    if (response.IsSuccessStatusCode)
                                                    {
                                                        return;
                                                    }
                                        
                                                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                                    var absoluteUrl = $"{httpClient.BaseAddress}{url}";
                                                    throw new ProblemDetailsException("Client call to endpoint was not successfull",
                                                                                      $"The http call: {httpMethod.Method} {url} was not successful",
                                                                                      ("HttpMethod", httpMethod.Method),
                                                                                      ("Url", absoluteUrl),
                                                                                      ("Error", content));
                                                }
                                        
                                                internal async Task<TResult> CallAsync<TResult>(Func<HttpClient, string, string, Task<HttpResponseMessage>> httpFunction,
                                                                                                HttpMethod httpMethod,
                                                                                                string url,
                                                                                                string paylod)
                                                {
                                                    var response = await httpFunction(httpClient, url, paylod).ConfigureAwait(false);
                                                    var result = await responseHandleStrategy.HandleAsync<TResult>(response, httpMethod, httpClient, url).ConfigureAwait(false);
                                                    return result;
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
            var file = Path.Combine(appFolder.FullName, "HttpCallHandler.cs");

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
