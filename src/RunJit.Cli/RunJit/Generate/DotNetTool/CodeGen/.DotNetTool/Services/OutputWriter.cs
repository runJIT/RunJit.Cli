using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddOutputWriterCodeGenExtension
    {
        internal static void AddOutputWriterCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, OutputWriterCodeGen>();
        }
    }

    internal sealed class OutputWriterCodeGen(ConsoleService consoleService,
                                              NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using Extensions.Pack;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            internal static class AddOutputWriterExtension
                                            {
                                                internal static void AddOutputWriter(this IServiceCollection services)
                                                {
                                                    services.AddConsoleService();
                                                
                                                    services.AddSingletonIfNotExists<OutputWriter>();
                                                }
                                            }
                                        
                                            internal sealed class OutputWriter(ConsoleService consoleService)
                                            {
                                                internal async Task WriteAsync(string value, 
                                                                               FileInfo? fileInfo,
                                                                               CancellationToken cancellationToken = default)
                                                {
                                                    if (fileInfo.IsNull())
                                                    {
                                                        consoleService.WriteSuccess(value);
                                                        return;
                                                    }
                                        
                                                    if (fileInfo.Directory.IsNull() ||
                                                        fileInfo.Directory.Name.IsNullOrWhiteSpace())
                                                    {
                                                        throw new ProblemDetailsException("Directory must not be NULL, Empty or Whitspace please check you passed value for your output.",
                                                                                          "Pelease check you passed value for your output.",
                                                                                          ("FileInfo", fileInfo.FullName),
                                                                                          ("Output", value));
                                                    }
                                        
                                                    if (fileInfo.Directory.NotExists())
                                                    {
                                                        fileInfo.Directory.Create();
                                                    }
                                        
                                                    await File.WriteAllTextAsync(fileInfo.FullName, value, cancellationToken).ConfigureAwait(false);
                                        
                                                    consoleService.WriteSuccess(fileInfo.FullName);
                                                }
                                            }
                                        }
                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add Services Folder
            var appFolder = new DirectoryInfo(Path.Combine(projectFileInfo.Directory!.FullName, "Services"));

            if (appFolder.NotExists())
            {
                appFolder.Create();
            }

            // 2. Add OutputWriter.cs
            var file = Path.Combine(appFolder.FullName, "OutputWriter.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName)
                                      .Replace("$dotnettoolnamelower$", dotNetToolInfos.NormalizedName.ToLower());

            var formattedTemplate = newTemplate;

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 3. Adjust namespace provider
            namespaceProvider.SetNamespaceProvider(projectFileInfo, $"{dotNetToolInfos.ProjectName}.Services", true);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
