using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddConsoleServiceCodeGenExtension
    {
        internal static void AddConsoleServiceCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, ConsoleServiceCodeGen>();
        }
    }

    internal sealed class ConsoleServiceCodeGen(ConsoleService consoleService,
                                                NamespaceProvider namespaceProvider) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using Extensions.Pack;

                                        namespace $namespace$
                                        {
                                            internal static class AddConsoleServiceExtension
                                            {
                                                internal static void AddConsoleService(this IServiceCollection services)
                                                {
                                                    services.AddSingletonIfNotExists<ConsoleService>();
                                                }
                                            }
                                        
                                            internal sealed class ConsoleService
                                            {
                                                internal void WriteLine()
                                                {
                                                    System.Console.WriteLine();
                                                }
                                        
                                                internal void WriteInfo(string value)
                                                {
                                                    WriteLine(value);
                                                }
                                        
                                                internal void WriteInput(string value)
                                                {
                                                    System.Console.ForegroundColor = ConsoleColor.Green;
                                                    WriteLine(value);
                                                    System.Console.ForegroundColor = ConsoleColor.White;
                                                }
                                        
                                                internal void WriteSuccess(string value)
                                                {
                                                    System.Console.ForegroundColor = ConsoleColor.Green;
                                                    WriteLine(value);
                                                    System.Console.ForegroundColor = ConsoleColor.White;
                                                }
                                        
                                                internal string ReadLine()
                                                {
                                                    var result = System.Console.ReadLine();
                                                    System.Console.WriteLine();
                                        
                                                    return result ?? string.Empty;
                                                }
                                        
                                                internal void WriteSample(string value)
                                                {
                                                    System.Console.ForegroundColor = ConsoleColor.Gray;
                                                    System.Console.WriteLine(value);
                                                    System.Console.WriteLine();
                                                    System.Console.ForegroundColor = ConsoleColor.White;
                                                }
                                        
                                                internal void WriteError(string value)
                                                {
                                                    System.Console.ForegroundColor = ConsoleColor.Red;
                                                    WriteLine(value);
                                                    System.Console.ForegroundColor = ConsoleColor.White;
                                                }
                                        
                                                private void WriteLine(string value)
                                                {
                                                    System.Console.WriteLine(value);
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

            // 2. Add ConsoleService.cs
            var file = Path.Combine(appFolder.FullName, "ConsoleService.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName);

            var formattedTemplate = newTemplate.FormatSyntaxTree();

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 3. Adjust namespace provider
            namespaceProvider.SetNamespaceProvider(projectFileInfo, $"{dotNetToolInfos.ProjectName}.Services", true);

            // 4. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
