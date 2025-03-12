using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddRequestTypeHandleStrategyCodeGenExtension
    {
        internal static void AddRequestTypeHandleStrategyCodeGen(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddNamespaceProvider();

            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, RequestTypeHandleStrategyCodeGen>();
        }
    }

    internal sealed class RequestTypeHandleStrategyCodeGen(ConsoleService consoleService,
                                                           NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using System.Collections.Immutable;
                                        using Extensions.Pack;
                                        using Microsoft.Extensions.Configuration;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            internal static class AddRequestTypeHandleStrategyExtension
                                            {
                                                internal static void AddRequestTypeHandleStrategy(this IServiceCollection services)
                                                {
                                                    services.AddSingletonIfNotExists<RequestTypeHandleStrategy>();
                                                }
                                            }
                                        
                                            internal interface ISpecificRequestTypeHandler
                                            {
                                                bool CanHandle<TResult>(HttpResponseMessage responseMessage);
                                        
                                                Task<TResult> HandleAsync<TResult>(HttpResponseMessage responseMessage,
                                                                                   HttpMethod httpMethod,
                                                                                   HttpClient httpClient,
                                                                                   string url);
                                            }
                                        
                                            internal sealed class RequestTypeHandleStrategy
                                            {
                                                private readonly IEnumerable<ISpecificResponseTypeHandler> _responseHandlers;
                                        
                                                public RequestTypeHandleStrategy(IEnumerable<ISpecificResponseTypeHandler> responseHandlers)
                                                {
                                                    _responseHandlers = responseHandlers;
                                                }
                                        
                                                public Task<TResult> HandleAsync<TResult>(HttpResponseMessage responseMessage,
                                                                                          HttpMethod httpMethod,
                                                                                          HttpClient httpClient,
                                                                                          string url)
                                                {
                                                    var responseHandlers = _responseHandlers.Where(handler => handler.CanHandle<TResult>(responseMessage)).ToImmutableList();
                                                    if (responseHandlers.IsEmpty())
                                                    {
                                                        throw new ProblemDetailsException("No response handler was found to handle expected response",
                                                            $"No response handler was found to handle expected response type: '{typeof(TResult).Name}'",
                                                            ("Response type", typeof(TResult).Name));
                                                    }
                                        
                                                    if (responseHandlers.Count > 1)
                                                    {
                                                        throw new ProblemDetailsException("More than one reponse handlers was found for expected response type",
                                                            $"For response type: '{typeof(TResult).Name}' {responseHandlers.Count} response handler was found",
                                                            ("Response type", typeof(TResult).Name),
                                                            ("ResponseHandlers", responseHandlers.Select(handler => handler.GetType().Name).ToImmutableList()));
                                                    }
                                        
                                                    var result = responseHandlers[0].HandleAsync<TResult>(responseMessage, httpMethod, httpClient, url);
                                                    return result;
                                                }
                                            }
                                        }

                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add RequestTypeHandleStrategy Folder
            var appFolder = new DirectoryInfo(Path.Combine(projectFileInfo.Directory!.FullName, "RequestTypeHandling"));

            if (appFolder.NotExists())
            {
                appFolder.Create();
            }

            // 2. Add RequestTypeHandleStrategy.cs
            var file = Path.Combine(appFolder.FullName, "RequestTypeHandleStrategy.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName);

            var formattedTemplate = newTemplate.FormatSyntaxTree();

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 3. Adjust namespace provider
            namespaceProvider.SetNamespaceProvider(projectFileInfo, $"{dotNetToolInfos.ProjectName}.RequestTypeHandling", true);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
