using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddAppBuilderCodeGenExtension
    {
        internal static void AddAppBuilderCodeGen(this IServiceCollection services)
        {
            services.AddConsoleService();

            services.AddSingletonIfNotExists<IDotNetToolSpecificCodeGen, AppBuilderCodeGen>();
        }
    }

    internal sealed class AppBuilderCodeGen(ConsoleService consoleService) : IDotNetToolSpecificCodeGen
    {
        private const string Template = """
                                        using Extensions.Pack;
                                        using Microsoft.Extensions.Configuration.Json;
                                        using Microsoft.Extensions.Configuration;
                                        using Microsoft.Extensions.DependencyInjection;

                                        namespace $namespace$
                                        {
                                            internal sealed class AppBuilder
                                            {
                                                internal App Build(Action<IServiceCollection, IConfiguration> serviceInterceptor)
                                                {
                                                    // 1. Setup startup
                                                    var startup = new Startup();
                                        
                                                    // 2. Setup service collection
                                                    var services = new ServiceCollection();
                                        
                                                    // 3. Setup configuration builder
                                                    var configurationBuilder = new ConfigurationBuilder();
                                        
                                                    // 3.1 We are using embedded appsettings cause of deployments of .net tool.
                                                    var appsettingsAsStream = typeof(AppBuilder).Assembly.GetEmbeddedFileAsStream("appsettings.json");
                                                    var jsonStreamConfigurationSource = new JsonStreamConfigurationSource
                                                    {
                                                        Stream = appsettingsAsStream
                                                    };
                                        
                                                    // 3.2 Add json stream configuration source
                                                    configurationBuilder.Add(jsonStreamConfigurationSource);
                                        
                                                    // 3.3 Add optional possibility to overwrite settings with appsettings.json in 
                                                    //     current directory / Manual post config after installation
                                                    configurationBuilder.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "appsettings.json"), optional: true);
                                        
                                                    // 3.4 Add environment variables. With this we can overwrite all used settings :)
                                                    configurationBuilder.AddEnvironmentVariables();
                                        
                                                    // 3.5 Add user secrets. With this we can overwrite all used settings :)
                                                    configurationBuilder.AddUserSecrets(typeof(AppBuilder).Assembly);
                                        
                                                    // 4. Build configuration
                                                    var configuration = configurationBuilder.Build();
                                        
                                                    // 5. Call startup to configure and setup the .net tool
                                                    startup.ConfigureServices(services, configuration);
                                        
                                                    // 6. Service interceptor for custom setups
                                                    serviceInterceptor(services, configuration);
                                                    
                                                    // 7. Build service provider
                                                    var buildServiceProvider = services.BuildServiceProvider();
                                                    
                                                    // 8. Provide the ready to use app
                                                    return new App(buildServiceProvider);
                                                }
                                            }
                                        }
                                        """;

        public async Task GenerateAsync(FileInfo projectFileInfo,
                                        XDocument projectDocument,
                                        DotNetToolInfos dotNetToolInfos)
        {
            // 1. Add App Folder
            var appFolder = new DirectoryInfo(Path.Combine(projectFileInfo.Directory!.FullName, "App"));

            if (appFolder.NotExists())
            {
                appFolder.Create();
            }

            // 2. Add AppBuilder.cs
            var file = Path.Combine(appFolder.FullName, "AppBuilder.cs");

            var newTemplate = Template.Replace("$namespace$", dotNetToolInfos.ProjectName)
                                      .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName);

            var formattedTemplate = newTemplate;

            await File.WriteAllTextAsync(file, formattedTemplate).ConfigureAwait(false);

            // 3. Print success message
            consoleService.WriteSuccess($"Successfully created {file}");
        }
    }
}
