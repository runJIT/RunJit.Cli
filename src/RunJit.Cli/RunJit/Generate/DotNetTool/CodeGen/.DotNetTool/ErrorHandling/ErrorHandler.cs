using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddErrorHandlerCodeGenExtension
    {
        internal static void AddErrorHandlerCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, ErrorHandlerCodeGen>();
        }
    }

    internal sealed class ErrorHandlerCodeGen(ConsoleService consoleService,
                                              NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using System.CommandLine.Invocation;
                                        using Extensions.Pack;
                                        using Microsoft.Extensions.Configuration;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            internal static class AddErrorHandlerExtension
                                            {
                                                internal static void AddErrorHandler(this IServiceCollection services)
                                                {
                                                    services.AddConsoleService();
                                            
                                                    services.AddSingletonIfNotExists<ErrorHandler>();
                                                }
                                            }

                                            internal sealed class ErrorHandler(ConsoleService consoleService)
                                            {
                                                internal async Task HandleErrorsAsync(InvocationContext context, Func<InvocationContext, Task> next)
                                                {
                                                    try
                                                    {
                                                        await next(context).ConfigureAwait(false);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        var ex = FindMostSuitableException(e);
                                                        if (ex is ProblemDetailsException problemDetailsException)
                                                        {
                                                            consoleService.WriteError(problemDetailsException.ProblemDetails.ToJsonIntended());
                                                        }
                                                        else
                                                        {
                                                            var problemDetails = new ProblemDetails
                                                            {
                                                                Title = $"Unexpected {ex.GetType().Name} occured",
                                                                Detail = ex.Message,
                                                                Status = 500
                                                            };
                                                            
                                                            consoleService.WriteError(problemDetails.ToJsonIntended());
                                                        }
                                                        
                                                        context.ResultCode = 1;
                                                    }
                                                }

                                                private static Exception FindMostSuitableException(Exception exception)
                                                {
                                                    if (exception is ProblemDetailsException)
                                                    {
                                                        return exception;
                                                    }

                                                    if (exception.InnerException != null)
                                                    {
                                                        return FindMostSuitableException(exception.InnerException);
                                                    }

                                                    return exception;
                                                }
                                            }
                                        }

                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add ErrorHandling Folder
            var appFolder = new DirectoryInfo(Path.Combine(projectFileInfo.Directory!.FullName, "ErrorHandling"));

            if (appFolder.NotExists())
            {
                appFolder.Create();
            }

            // 2. Add ErrorHandler.cs
            var file = Path.Combine(appFolder.FullName, "ErrorHandler.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName);

            var formattedTemplate = newTemplate.FormatSyntaxTree();

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 3. Adjust namespace provider
            namespaceProvider.SetNamespaceProvider(projectFileInfo, $"{dotNetToolInfos.ProjectName}.ErrorHandling", true);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
