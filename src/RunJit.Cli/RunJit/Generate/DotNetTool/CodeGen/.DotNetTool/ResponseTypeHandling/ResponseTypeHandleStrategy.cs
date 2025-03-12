using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddResponseTypeHandleStrategyCodeGenExtension
    {
        internal static void AddResponseTypeHandleStrategyCodeGen(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddNamespaceProvider();

            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, ResponseTypeHandleStrategyCodeGen>();
        }
    }

    internal sealed class ResponseTypeHandleStrategyCodeGen(ConsoleService consoleService,
                                                            NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using System.Collections.Immutable;
                                        using Extensions.Pack;
                                        using Microsoft.Extensions.Configuration;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            internal static class AddResponseTypeHandleStrategyExtension
                                            {
                                                internal static void AddResponseTypeHandleStrategy(this IServiceCollection services)
                                                {
                                                    services.AddNotOkResponseTypeHandler();
                                                    services.AddJsonResponseTypeHandler();
                                                    services.AddFileStreamResponseTypeHandler();
                                                    services.AddByteArrayResponseTypeHandler();
                                        
                                                    services.AddSingletonIfNotExists<ResponseTypeHandleStrategy>();
                                                }
                                            }
                                        
                                            internal interface ISpecificResponseTypeHandler
                                            {
                                                bool CanHandle<TResult>(HttpResponseMessage responseMessage);
                                        
                                                Task<TResult> HandleAsync<TResult>(HttpResponseMessage responseMessage,
                                                                                   HttpMethod httpMethod,
                                                                                   HttpClient httpClient,
                                                                                   string url);
                                            }
                                        
                                            internal sealed class ResponseTypeHandleStrategy(IEnumerable<ISpecificResponseTypeHandler> responseHandlers)
                                            {
                                                public Task<TResult> HandleAsync<TResult>(HttpResponseMessage responseMessage,
                                                                                          HttpMethod httpMethod,
                                                                                          HttpClient httpClient,
                                                                                          string url)
                                                {
                                                    var matchingResponseHandlers = responseHandlers.Where(handler => handler.CanHandle<TResult>(responseMessage)).ToImmutableList();
                                                    if (matchingResponseHandlers.IsEmpty())
                                                    {
                                                        throw new ProblemDetailsException("No response handler was found to handle expected response",
                                                                                          $"No response handler was found to handle expected response type: '{typeof(TResult).Name}'",
                                                                                          ("Response type", typeof(TResult).Name));
                                                    }
                                        
                                                    if (matchingResponseHandlers.Count > 1)
                                                    {
                                                        throw new ProblemDetailsException("More than one reponse handlers was found for expected response type",
                                                                                          $"For response type: '{typeof(TResult).Name}' {matchingResponseHandlers.Count} response handler was found",
                                                                                          ("Response type", typeof(TResult).Name),
                                                                                          ("ResponseHandlers", matchingResponseHandlers.Select(handler => handler.GetType().Name).ToImmutableList()));
                                                    }
                                        
                                                    var result = matchingResponseHandlers[0].HandleAsync<TResult>(responseMessage, httpMethod, httpClient, url);
                                                    return result;
                                                }
                                            }
                                        }
                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add ResponseTypeHandleStrategy Folder
            var appFolder = new DirectoryInfo(Path.Combine(projectFileInfo.Directory!.FullName, "ResponseTypeHandling"));

            if (appFolder.NotExists())
            {
                appFolder.Create();
            }

            // 2. Add ResponseTypeHandleStrategy.cs
            var file = Path.Combine(appFolder.FullName, "ResponseTypeHandleStrategy.cs");

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
