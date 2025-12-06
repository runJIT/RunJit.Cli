using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddProblemDetailsExceptionCodeGenExtension
    {
        internal static void AddProblemDetailsExceptionCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, ProblemDetailsExceptionCodeGen>();
        }
    }

    internal sealed class ProblemDetailsExceptionCodeGen(ConsoleService consoleService,
                                                         NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using System.Collections.Immutable;
                                        using System.Net;
                                        using Extensions.Pack;
                                        using Microsoft.Extensions.Configuration;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            internal sealed class ProblemDetailsException : Exception
                                            {
                                                internal ProblemDetailsException(string title,
                                                                                 params (string key, object value)[] extensions) : this(HttpStatusCode.InternalServerError, title, string.Empty, extensions)
                                                {
                                                }

                                                internal ProblemDetailsException(string title,
                                                                                 string details,
                                                                                 params (string key, object value)[] extensions) : this(HttpStatusCode.InternalServerError, title, details, extensions)
                                                {
                                                }

                                                internal ProblemDetailsException(HttpStatusCode statusCode,
                                                                                 string title,
                                                                                 string details,
                                                                                 params (string key, object value)[] extensions) : this(statusCode.ToInt(), title, details, extensions.ToImmutableDictionary(item => item.key, item => item.value))
                                                {
                                                }

                                                internal ProblemDetailsException(int statusCode,
                                                                                 string title,
                                                                                 string details,
                                                                                 params (string key, object value)[] extensions) : this(statusCode.ToInt(), title, details, extensions.ToImmutableDictionary(item => item.key, item => item.value))
                                                {
                                                }

                                                // i know this is evil with the conversion to immutable dictionary but a fast fix for now.

                                                internal ProblemDetailsException(int statusCode,
                                                                                 string title,
                                                                                 string details,
                                                                                 IImmutableDictionary<string, object> errorDetails) : base(title)
                                                {
                                                    var problemDetails = new ProblemDetails { Title = title.IsEmpty() ? null : title, Detail = details.IsEmpty() ? null : details, Status = statusCode };

                                                    errorDetails.OrderBy(item => item.Key).ForEach(keyValue =>
                                                    {
                                                        var key = keyValue.Key.Split(" ").Select(value => value.FirstCharToUpper()).Flatten().FirstCharToLower();
                                                        problemDetails.Extensions.Add(key, keyValue.Value);
                                                    });

                                                    ProblemDetails = problemDetails;
                                                }

                                                internal ProblemDetails ProblemDetails { get; }
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

            // 2. Add ProblemDetailsException.cs
            var file = Path.Combine(appFolder.FullName, "ProblemDetailsException.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName);

            var formattedTemplate = newTemplate;

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 3. Adjust namespace provider
            namespaceProvider.SetNamespaceProvider(projectFileInfo, $"{dotNetToolInfos.ProjectName}.ErrorHandling", true);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
