using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddProgramCodeGenExtension
    {
        internal static void AddProgramCodeGen(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, ProgramCodeGen>();
        }
    }

    internal sealed class ProgramCodeGen(ConsoleService consoleService) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using Microsoft.Extensions.Configuration;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            // Needed public to run automated tests which are debugable
                                            public static class Program
                                            {
                                                public static Task<int> Main(string[] args)
                                                {
                                                    return CustomSetupAsync(args, (_, __) => { });
                                                }
                                            
                                                public static Task<int> CustomSetupAsync(string[] args,
                                                                                         Action<IServiceCollection, IConfiguration> serviceInterceptor)
                                                {
                                                    return new AppBuilder().Build(serviceInterceptor).RunAsync(args);
                                                }
                                            }
                                        }
                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add AppBuilder.cs
            var file = Path.Combine(projectFileInfo.Directory!.FullName, "Program.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName);

            var formattedTemplate = newTemplate.FormatSyntaxTree();

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 2. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
