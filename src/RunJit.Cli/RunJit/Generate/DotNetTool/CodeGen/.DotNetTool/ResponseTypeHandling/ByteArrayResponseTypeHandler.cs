using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddByteArrayResponseTypeHandlerCodeGenExtension
    {
        internal static void AddByteArrayResponseTypeHandlerCodeGen(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddNamespaceProvider();

            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, ByteArrayResponseTypeHandlerCodeGen>();
        }
    }

    internal sealed class ByteArrayResponseTypeHandlerCodeGen(ConsoleService consoleService,
                                                              NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using Extensions.Pack;

                                        namespace $namespace$
                                        {
                                            internal static class AddByteArrayResponseTypeHandlerExtension
                                            {
                                                internal static void AddByteArrayResponseTypeHandler(this IServiceCollection services)
                                                {
                                                    services.AddSingletonIfNotExists<ISpecificResponseTypeHandler, ByteArrayResponseTypeHandler>();
                                                }
                                            }
                                        
                                            internal sealed class ByteArrayResponseTypeHandler : ISpecificResponseTypeHandler
                                            {
                                                public bool CanHandle<TResult>(HttpResponseMessage responseMessage)
                                                {
                                                    return responseMessage.IsSuccessStatusCode &&
                                                           typeof(TResult) == typeof(byte[]);
                                                }
                                        
                                                public async Task<TResult> HandleAsync<TResult>(HttpResponseMessage responseMessage,
                                                                                                HttpMethod httpMethod,
                                                                                                HttpClient httpClient,
                                                                                                string url)
                                                {
                                                    // Safety first this method can be called without CanHandle check !
                                                    if (CanHandle<TResult>(responseMessage).IsFalse())
                                                    {
                                                        throw new ProblemDetailsException("ByteArrayResponseTypeHandler was called without checking CanHandle.",
                                                                                          $"The response type handler for type: {typeof(TResult).Name} is not supported",
                                                                                          ("SupportedTypes", "byte[]"));
                                                    }
                                                    
                                                    var result = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                                                    return (TResult)result.Cast<object>();
                                                }
                                            }
                                        }

                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add ByteArrayResponseTypeHandler Folder
            var appFolder = new DirectoryInfo(Path.Combine(projectFileInfo.Directory!.FullName, "ResponseTypeHandling"));

            if (appFolder.NotExists())
            {
                appFolder.Create();
            }

            // 2. Add ByteArrayResponseTypeHandler.cs
            var file = Path.Combine(appFolder.FullName, "ByteArrayResponseTypeHandler.cs");

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
