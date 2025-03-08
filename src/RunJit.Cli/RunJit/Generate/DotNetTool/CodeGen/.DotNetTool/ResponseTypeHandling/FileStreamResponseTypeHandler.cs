using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddFileStreamResponseTypeHandlerCodeGenExtension
    {
        internal static void AddFileStreamResponseTypeHandlerCodeGen(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddNamespaceProvider();

            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, FileStreamResponseTypeHandlerCodeGen>();
        }
    }

    internal sealed class FileStreamResponseTypeHandlerCodeGen(ConsoleService consoleService,
                                                               NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using System.Net.Mime;
                                        using Extensions.Pack;
                                        using Microsoft.AspNetCore.Mvc;

                                        namespace $namespace$
                                        {
                                            internal static class AddFileStreamResponseTypeHandlerExtension
                                            {
                                                internal static void AddFileStreamResponseTypeHandler(this IServiceCollection services)
                                                {
                                                    services.AddSingletonIfNotExists<ISpecificResponseTypeHandler, FileStreamResponseType>();
                                                }
                                            }
                                        
                                            internal sealed class FileStreamResponseType : ISpecificResponseTypeHandler
                                            {
                                                public async Task<TResult> HandleAsync<TResult>(HttpResponseMessage responseMessage,
                                                                                                HttpMethod httpMethod,
                                                                                                HttpClient httpClient,
                                                                                                string url)
                                                {
                                                    // Safety first this method can be called without CanHandle check !
                                                    if (CanHandle<TResult>(responseMessage).IsFalse())
                                                    {
                                                        throw new ProblemDetailsException("FileStreamResponseTypeHandler was called without checking CanHandle.",
                                                                                          $"The response type handler for type: {typeof(TResult).Name} is not supported",
                                                                                          ("SupportedTypes", "FileStreamResult"));
                                                    }
                                                    
                                                    var content = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
                                                    var fileName = responseMessage.Content.Headers.ContentDisposition?.FileName ?? string.Empty;
                                                    var fileStreamResult = new FileStreamResult(content, MediaTypeNames.Application.Octet) { FileDownloadName = fileName };
                                                    return fileStreamResult.Cast<TResult>();
                                                }
                                        
                                                public bool CanHandle<TResult>(HttpResponseMessage responseMessage)
                                                {
                                                    return responseMessage.IsSuccessStatusCode &&
                                                           typeof(TResult) == typeof(FileStreamResult);
                                                }
                                            }
                                        }

                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add FileStreamResponseTypeHandler Folder
            var appFolder = new DirectoryInfo(Path.Combine(projectFileInfo.Directory!.FullName, "ResponseTypeHandling"));

            if (appFolder.NotExists())
            {
                appFolder.Create();
            }

            // 2. Add FileStreamResponseTypeHandler.cs
            var file = Path.Combine(appFolder.FullName, "FileStreamResponseTypeHandler.cs");

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
