using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddNotOkResponseTypeHandlerCodeGenExtension
    {
        internal static void AddNotOkResponseTypeHandlerCodeGen(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddNamespaceProvider();

            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, NotOkResponseTypeHandlerCodeGen>();
        }
    }

    internal sealed class NotOkResponseTypeHandlerCodeGen(ConsoleService consoleService,
                                                          NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using Extensions.Pack;
                                        using Microsoft.Extensions.Configuration;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            internal static class AddNotOkResponseTypeHandlerExtension
                                            {
                                                internal static void AddNotOkResponseTypeHandler(this IServiceCollection services)
                                                {
                                                    services.AddSingletonIfNotExists<ISpecificResponseTypeHandler, NotOkResponseTypeHandler>();
                                                }
                                            }

                                            internal sealed class NotOkResponseTypeHandler : ISpecificResponseTypeHandler
                                            {
                                                public bool CanHandle<TResult>(HttpResponseMessage responseMessage)
                                                {
                                                    return responseMessage.IsSuccessStatusCode.IsFalse();
                                                }

                                                public async Task<TResult> HandleAsync<TResult>(HttpResponseMessage responseMessage,
                                                                                                HttpMethod httpMethod,
                                                                                                HttpClient httpClient,
                                                                                                string url)
                                                {
                                                    // Safety first this method can be called without CanHandle check !
                                                    if (CanHandle<TResult>(responseMessage).IsFalse())
                                                    {
                                                        throw new ProblemDetailsException("NotOkResponseTypeHandler was called without checking CanHandle.",
                                                                                          $"The response type handler for type: {typeof(TResult).Name} is not supported",
                                                                                          ("SupportedTypes", "NotOk responses"));
                                                    }
                                                    
                                                    var content = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                                                    var absoluteUrl = $"{httpClient.BaseAddress}{url}";
                                                    throw new ProblemDetailsException(responseMessage.StatusCode,
                                                                                      "Client call to endpoint was not successfull",
                                                                                      $"The http call: {httpMethod.Method} {url} was not succesfull",
                                                                                      ("HttpMethod", httpMethod.Method),
                                                                                      ("Url", absoluteUrl),
                                                                                      ("Error", content));
                                                }
                                            }
                                        }

                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add NotOkResponseTypeHandler Folder
            var appFolder = new DirectoryInfo(Path.Combine(projectFileInfo.Directory!.FullName, "ResponseTypeHandling"));

            if (appFolder.NotExists())
            {
                appFolder.Create();
            }

            // 2. Add NotOkResponseTypeHandler.cs
            var file = Path.Combine(appFolder.FullName, "NotOkResponseTypeHandler.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName);

            var formattedTemplate = newTemplate;

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 3. Adjust namespace provider
            namespaceProvider.SetNamespaceProvider(projectFileInfo, $"{dotNetToolInfos.ProjectName}.ResponseTypeHandling", true);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
