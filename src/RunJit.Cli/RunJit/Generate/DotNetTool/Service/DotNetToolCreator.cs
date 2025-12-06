using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.DotNetTool.Models;
using RunJit.Cli.RunJit.Generate.Client;
using RunJit.Cli.Services;
using RunJit.Cli.Services.Endpoints;
using Solution.Parser.CSharp;
using Solution.Parser.Solution;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddDotnetToolGenExtension
    {
        internal static void AddDotnetToolGen(this IServiceCollection services)
        {
            // App
            services.AddAppCodeGen();
            services.AddAppBuilderCodeGen();

            // ErrorHandling
            services.AddErrorHandlerCodeGen();
            services.AddProblemDetailsCodeGen();
            services.AddProblemDetailsExceptionCodeGen();

            // Appsettings
            services.AddAppSettingsCodeGen();

            // Argument
            services.AddArgumentBuilderCodeGen();

            // Services
            services.AddOutputFormatterCodeGen();
            services.AddOutputServiceCodeGen();
            services.AddOutputWriterCodeGen();

            services.AddConsoleServiceCodeGen();
            services.AddProgramCodeGen();
            services.AddStartupCodeGen();
            services.AddCommandCodeGen();
            services.AddArgumentFixerCodeGen();
            services.AddProjectSettingsCodeGen();

            services.AddProjectEmbeddedFilesCodeGen();

            // services.AddProjectTypeCodeGen();

            // HttpCallHandlers
            services.AddHttpCallHandlerCodeGen();
            services.AddHttpCallHandlerFactoryCodeGen();
            services.AddHttpClientFactoryCodeGen();
            services.AddHttpRequestMessageBuilderCodeGen();

            // RequestTypeHandling
            // Not needed at the moment
            services.AddRequestTypeHandleStrategyCodeGen();

            // ResponseTypeHandling
            services.AddByteArrayResponseTypeHandlerCodeGen();
            services.AddFileStreamResponseTypeHandlerCodeGen();
            services.AddJsonResponseTypeHandlerCodeGen();
            services.AddNotOkResponseTypeHandlerCodeGen();
            services.AddResponseTypeHandleStrategyCodeGen();
        }
    }

    internal static class AddDotNetToolCreatorExtension
    {
        internal static void AddDotNetToolCreator(this IServiceCollection services)
        {
            services.AddControllerParser();
            services.AddApiTypeLoader();
            services.AddRestructureController();
            services.AddEndpointsToCliCommandStructureConverter();
            services.AddMinimalApiEndpointParser();
            services.AddOrganizeMinimalEndpoints();
            services.AddDotNetToolCodeGen();

            services.AddSingletonIfNotExists<DotNetToolCreator>();
        }
    }

    internal sealed class DotNetToolCreator(ControllerParser controllerParser,
                                            ApiTypeLoader apiTypeLoader,
                                            RestructureController restructureController,
                                            EndpointsToCliCommandStructureConverter convertControllerInfos,
                                            MinimalApiEndpointParser minimalApiEndpointParser,
                                            OrganizeMinimalEndpoints organizeMinimalEndpoints,
                                            DotNetToolCodeGen dotNetToolCodeGen,
                                            FindUsedNetVersion findUsedNetVersion)
    {
        internal async Task GenerateDotNetToolAsync(string projectName,
                                                    string name,
                                                    string normalizedName,
                                                    FileInfo clientSolution)
        {
            // 1. Build the target solution first
            //await dotNet.BuildAsync(clientSolution).ConfigureAwait(false);

            // 2. Parse the solution
            var parsedSolution = new SolutionFileInfo(clientSolution.FullName).Parse();

            // 3. Parse all C# files
            var allSyntaxTrees = parsedSolution.ProductiveProjects.SelectMany(p => p.CSharpFileInfos.Select(c => c.Parse())).ToImmutableList();

            // 4. Get all types which are declared in the API assembly - Need to unique ident the types for client generation.
            var types = apiTypeLoader.GetAllTypesFrom(parsedSolution);

            // 5. New to reorganize the controllers to get the correct domain name
            var controllerInfosOrg = controllerParser.ExtractFrom(allSyntaxTrees, types).OrderBy(controller => controller.Name).ToImmutableList();

            // 6. New to reorganize the controllers to get the correct domain name
            var controllerInfos = restructureController.Reorganize(controllerInfosOrg);

            // 7. Get all minimal endpoints
            var endpoints = minimalApiEndpointParser.ExtractFrom(allSyntaxTrees, types, false).OrderBy(controller => controller.Name).ToImmutableList();

            // 8. organize endpoints
            var organizedEndpoints = organizeMinimalEndpoints.Reorganize(endpoints);

            // 9. Covert to mapped endpoints
            //    We want to have them grouped by domain to get correct tree like:
            //    - Users 
            //      - V1  
            //      - V2
            var mappedToEndpoints = organizedEndpoints.Any() ? organizedEndpoints : controllerInfos.ToEndpointInfos();
            var domainGrouped = mappedToEndpoints.GroupBy(e => e.GroupName).Distinct().ToImmutableList();

            // 10. Get .Net version
            var netVersion = findUsedNetVersion.GetNetVersion(parsedSolution);

            // 10. Convert controller infos to dotnet tool structure -> commands, arguments and options
            var basePath = minimalApiEndpointParser.FindBasePath(allSyntaxTrees);

            var dotnetToolStructure = convertControllerInfos.ConvertTo(domainGrouped, projectName, name,
                                                                       normalizedName, netVersion, basePath);

            // 11. Run all code generators
            await dotNetToolCodeGen.GenerateAsync(parsedSolution, dotnetToolStructure).ConfigureAwait(false);
        }
    }

    internal static class AddConvertControllerInfosToDotnetToolStructureExtension
    {
        internal static void AddEndpointsToCliCommandStructureConverter(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<EndpointsToCliCommandStructureConverter>();
        }
    }

    internal sealed class EndpointsToCliCommandStructureConverter
    {
        private const string GetConfigTemplate = """
                                                 using Extensions.Pack;
                                                 using Microsoft.Extensions.Configuration;
                                                 using Microsoft.Extensions.DependencyInjection;

                                                 namespace $namespace$
                                                 {
                                                     internal static class Add$command-name$HandlerExtension
                                                     {
                                                         internal static void Add$command-name$Handler(this IServiceCollection services, IConfiguration configuration)
                                                         {
                                                             services.AddOutputService();
                                                             services.Add$dotNetToolName$HttpClientFactory(configuration);

                                                             services.AddSingletonIfNotExists<$command-name$Handler>();
                                                         }
                                                     }

                                                     internal sealed class $command-name$Handler(OutputService outputService)
                                                     {
                                                         internal async Task HandleAsync($command-name$Parameters getParameters, CancellationToken cancellationToken = default)
                                                         {
                                                             // 1. If not provide the embedded version
                                                             var appsettings = EmbeddedFile.$command-name$FileContentFrom("appsettings.json");

                                                             // 2. Check if an appsettings.json exists on file
                                                             var appsettingsOnDisk = new FileInfo(Path.Combine(Environment.CurrentDirectory, "appsettings.json"));
                                                             if (appsettingsOnDisk.Exists)
                                                             {
                                                                 appsettings = await File.ReadAllTextAsync(appsettingsOnDisk.FullName, cancellationToken).ConfigureAwait(false);
                                                             }

                                                             // 3. Write the formatted string to the output
                                                             await outputService.WriteAsync(appsettings, getParameters.Output, getParameters.Format, cancellationToken).ConfigureAwait(false);
                                                         }
                                                     }
                                                 }
                                                 """;

        private const string SetConfigTemplate = """
                                                 using Extensions.Pack;
                                                 using Microsoft.Extensions.Configuration;
                                                 using Microsoft.Extensions.DependencyInjection;

                                                 namespace $namespace$
                                                 {
                                                     internal static class Add$command-name$HandlerExtension
                                                     {
                                                         internal static void Add$command-name$Handler(this IServiceCollection services, IConfiguration _)
                                                         {
                                                             services.AddOutputService();
                                                         
                                                             services.AddSingletonIfNotExists<$command-name$Handler>();
                                                         }
                                                     }

                                                     internal sealed class $command-name$Handler(OutputService outputService)
                                                     {
                                                         internal async Task HandleAsync($command-name$Parameters setParameters, CancellationToken cancellationToken = default)
                                                         {
                                                             // 1. Define target file
                                                             var appsettingsOnDisk = new FileInfo(Path.Combine(Environment.CurrentDirectory, "appsettings.json"));

                                                             // 2. Write output
                                                             await outputService.WriteAsync(setParameters.Json, appsettingsOnDisk, FormatType.JsonIndented, cancellationToken).ConfigureAwait(false);
                                                         }
                                                     }
                                                 }
                                                 """;

        public DotNetToolInfos ConvertTo(ImmutableList<IGrouping<string, EndpointGroup>> domainGroupedByVersion,
                                         string projectName,
                                         string name,
                                         string normalizedName,
                                         string netVersion,
                                         string basePath)
        {
            // Root command is the first command
            // Sample:
            // - dotnet tool install (dotnet is root command)
            // - pulse core resources v1 (pulse is the root command)
            var rootCommand = new CommandInfo
                              {
                                  NormalizedName = normalizedName,
                                  Value = name,
                                  Name = name,
                                  Description = name
                              };

            // Common options
            var tokenOption = new OptionInfo
                              {
                                  Alias = "-t",
                                  NormalizedName = "token",
                                  Argument = new ArgumentInfo("token", "Bearer token for authentication", "<token>[string]",
                                                              "string", "string", "Token"),
                                  IsIsRequired = false,
                                  Name = "token",
                                  Value = "--token",
                                  Description = "Bearer token for authentication"
                              };

            // New options for output and formating !
            var options = ImmutableList.Create(new OptionInfo
                                               {
                                                   Alias = "-f",
                                                   NormalizedName = "format",
                                                   Argument = new ArgumentInfo("format", "Provide the format in which type the output should be created. Choose an an available option", "<format>[FormatType]",
                                                                               "FormatType", "FormatType", "Format"),
                                                   IsIsRequired = false,
                                                   Name = "format",
                                                   Value = "--format",
                                                   Description = "Provide the format in which type the output should be created. Choose an an available option"
                                               },
                                               new OptionInfo
                                               {
                                                   Alias = "-o",
                                                   NormalizedName = "Output",
                                                   Argument = new ArgumentInfo("output", "Writes the output in your provided file. Sample: 'D:/Documents/MyFile.txt'", "<output>[FileInfo?]",
                                                                               "FileInfo?", "FileInfo?", "Output"),
                                                   IsIsRequired = false,
                                                   Name = "output",
                                                   Value = "--output",
                                                   Description = "Writes the output in your provided file. Sample: 'D:/Documents/MyFile.txt'"
                                               });

            // Config command
            // ToDo: We have to implement ICustomCommand to handle such exception cases :)

            // For your tool we want to be able to manage the whole configuration
            // So we need a command to get/set the configuration
            var configCommand = new CommandInfo
                                {
                                    NormalizedName = "Config",
                                    Value = "config",
                                    Name = "config",
                                    Description = "Command to handle the configuration for your dotnet tool. Known as appsettings"
                                };

            // We need a command to read the whole config
            var getConfigCommand = new CommandInfo
                                   {
                                       NormalizedName = "Get",
                                       Value = "get",
                                       Name = "get",
                                       Description = "Command to read the whole configuration for your dotnet tool. Known as appsettings",
                                       Options = options.ToList(),
                                       CodeTemplate = GetConfigTemplate,
                                       NoSyntaxTreeFormatting = true
                                   };

            // We need a command to set/update the whole config
            var setConfigCommand = new CommandInfo
                                   {
                                       NormalizedName = "Set",
                                       Value = "set",
                                       Name = "set",
                                       Description = "Command to set the whole configuration for your dotnet tool. Known as appsettings. To set the configuration call 'configuration get' change the settings you want to change and call 'configuration set' to set the whole json back",
                                       Options = options.ToList(),
                                       CodeTemplate = SetConfigTemplate,
                                       NoSyntaxTreeFormatting = true,
                                       Argument = new ArgumentInfo("json", "Provide the new config json which is known as appsettings.json", "<json>[string]",
                                                                   "string", "string", "Json")
                                   };

            configCommand.SubCommands.Add(getConfigCommand);
            configCommand.SubCommands.Add(setConfigCommand);

            // Health command
            // ToDo: We have to implement ICustomCommand to handle such exception cases :)
            // Health command does not exist es explicit endpoint it is just configured as
            // services.AddHealthChecks();
            // app.UseHealthChecks("/health);
            // For your tool we want to be able to manage the whole configuration
            // So we need a command to get/set the configuration
            var healthCommand = new CommandInfo
                                {
                                    NormalizedName = "Health",
                                    Value = "health",
                                    Name = "health",
                                    Description = "Command to get info about health status"
                                };

            // We need a command to read the whole config
            var getHealthCommand = new CommandInfo
                                   {
                                       NormalizedName = "GetHealthStatus",
                                       Value = "getHealthStatus",
                                       Name = "getHealthStatus",
                                       Description = "Get the health status of your api",
                                       Options = options.Add(tokenOption).ToList(),

                                       // CodeTemplate = GetConfigTemplate,
                                       EndpointInfo = new EndpointInfo
                                                      {
                                                          BaseUrl = basePath,
                                                          DomainName = "Health",
                                                          GroupName = "Health",
                                                          HttpAction = "Get",
                                                          ResponseType = new ResponseType("HealthStatusResponse",
                                                                                          "HealthStatusResponse"),
                                                          ProduceResponseTypes = ImmutableList<ProduceResponseTypes>.Empty,
                                                          RelativeUrl = "health",
                                                          Name = "GetHealthStatusAsync",
                                                          Models = new DeclarationBase("HealthStatusResponse",
                                                                                       "HealthStatusResponse",
                                                                                       """
                                                                                       internal sealed record HealthStatusResponse(string Status,
                                                                                       string TotalDuration,
                                                                                       Dictionary<string, object> Entries);
                                                                                       """,
                                                                                       string.Empty).AsImmutableList(),
                                                          Version = null,
                                                          SwaggerOperationId = "getHealthStatus",
                                                          Parameters = ImmutableList<Parameter>.Empty,
                                                          RequestType = null,
                                                          ObsoleteInfo = null
                                                      },
                                       NoSyntaxTreeFormatting = true
                                   };

            healthCommand.SubCommands.Add(getHealthCommand);

            rootCommand.SubCommands.Add(configCommand);
            rootCommand.SubCommands.Add(healthCommand);

            // convert controller infos into dotnet tool structure
            // endpointGroups are grouped by version
            foreach (var domainGroupByVersion in domainGroupedByVersion)
            {
                // Domain command like
                // Users
                // Resources
                var domainCommand = new CommandInfo
                                    {
                                        Name = domainGroupByVersion.Key,
                                        NormalizedName = domainGroupByVersion.Key,
                                        Description = $"Here comes the description for {domainGroupByVersion.Key}",
                                        Value = domainGroupByVersion.Key
                                    };

                foreach (var version in domainGroupByVersion)
                {
                    if (version.IsNull())
                    {
                        continue;
                    }

                    if (version.Version.IsNull())
                    {
                        continue;
                    }

                    // Domain command like
                    // Users
                    //  - V1
                    // Resources
                    //  - V1
                    var versionCommand = new CommandInfo
                                         {
                                             Name = version.Version.Normalized,
                                             NormalizedName = version.Version.Normalized,
                                             Description = $"Here comes the description for {version.Version.Normalized}",
                                             Value = version.Version.Normalized
                                         };

                    domainCommand.SubCommands.Add(versionCommand);

                    // each domain is a command
                    foreach (var endoint in version.Endpoints)
                    {
                        // Domain command like
                        // Users
                        //  - V1
                        //    - AddUser
                        // Resources
                        //  - V1
                        //    - AddResource
                        // Temp tool build do not allow exceptions
                        var optionsForEndpoints = options.Add(tokenOption).ToList();

                        var endpointCommand = new CommandInfo
                                              {
                                                  Name = endoint.SwaggerOperationId.FirstCharToUpper(),
                                                  NormalizedName = endoint.SwaggerOperationId.FirstCharToUpper(),
                                                  Description = $"Here comes the description for {endoint.SwaggerOperationId.FirstCharToUpper()}",
                                                  Value = endoint.SwaggerOperationId.FirstCharToUpper(),
                                                  Argument = endoint.Parameters.IsEmpty()
                                                                 ? null
                                                                 : new ArgumentInfo("json", "Call info as json which contains url- and query parameters as well the payload if needed", "<callInfos>[string]",
                                                                                    "string", "string", "Json"),
                                                  EndpointInfo = endoint,
                                                  Options = optionsForEndpoints
                                              };

                        versionCommand.SubCommands.Add(endpointCommand);
                    }
                }

                rootCommand.SubCommands.Add(domainCommand);
            }

            var dotnetToolInfos = new DotNetToolInfos
                                  {
                                      ProjectName = projectName,
                                      Name = name,
                                      NormalizedName = normalizedName,
                                      CommandInfo = rootCommand,
                                      NetVersion = netVersion
                                  };

            return dotnetToolInfos;
        }
    }
}
