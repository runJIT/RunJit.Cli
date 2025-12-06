using DotNetTool.Service;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Generate.Client;
using Solution.Parser.Project;
using Solution.Parser.Solution;

namespace RunJit.Cli.RunJit.Generate.Client
{
    internal static class AddTestStructureWriterExtension
    {
        internal static void AddTestStructureWriter(this IServiceCollection services)
        {
            services.AddMsTestBaseClassBuilder();
            services.AddAppsettingsBuilder();

            services.AddSingletonIfNotExists<TestStructureWriter>();
        }
    }

    internal sealed class TestStructureWriter(MsTestBaseClassBuilder msTestBaseClassBuilder,
                                              AppsettingsBuilder appsettingsBuilder,
                                              JsonSerializerBuilder jsonSerializerBuilder)
    {
        private const string HealthTestTemplate = """
                                                  using System.Collections.Immutable;
                                                  using AspNetCore.Simple.MsTest.Sdk;
                                                  using Microsoft.VisualStudio.TestTools.UnitTesting;

                                                  namespace $namespace$.Api.Health
                                                  {
                                                      [TestClass]
                                                      [TestCategory("Health")]
                                                      public class GetHealthStateTest : ApiTestBase
                                                      {
                                                          [TestMethod]
                                                          public async Task Should_Return_Healthy_State()
                                                          {
                                                              var healthResponse = await $clientName$.Health.GetHealthStatusAsync().ConfigureAwait(false);

                                                              Assert.That.ObjectsAreEqual("Healthy.json",
                                                                                          healthResponse,
                                                                                          differenceFunc: IgnoreDifferences);
                                                          }

                                                          private IEnumerable<Difference> IgnoreDifferences(ImmutableList<Difference> differences)
                                                          {
                                                              foreach (var difference in differences)
                                                              {
                                                                  if (difference.MemberPath.Contains("TotalDuration"))
                                                                  {
                                                                      continue;
                                                                  }

                                                                  yield return difference;
                                                              }
                                                          }
                                                      }
                                                  }

                                                  """;

        private const string HealthResponseFile = """
                                                  {
                                                    "Content": {
                                                      "Headers": [
                                                        {
                                                          "Key": "Expires",
                                                          "Value": [ "Thu, 01 Jan 1970 00:00:00 GMT" ]
                                                        },
                                                        {
                                                          "Key": "Content-Type",
                                                          "Value": [ "application/json" ]
                                                        }
                                                      ],
                                                      "Value": {
                                                        "status": "Healthy",
                                                        "totalDuration": "00:00:00.0006638",
                                                        "entries": {}
                                                      }
                                                    },
                                                    "StatusCode": "OK",
                                                    "Headers": [
                                                      {
                                                        "Key": "Cache-Control",
                                                        "Value": [ "no-store, no-cache" ]
                                                      },
                                                      {
                                                        "Key": "Pragma",
                                                        "Value": [ "no-cache" ]
                                                      }
                                                    ],
                                                    "TrailingHeaders": [],
                                                    "IsSuccessStatusCode": true
                                                  }

                                                  """;

        private const string EnvironmentVariables = """
                                                    {
                                                      "ASPNETCORE_ENVIRONMENT": "Development",
                                                      "Logging__LogLevel__Default": "Information",
                                                      "Logging__LogLevel__Microsoft.AspNetCore": "Warning",
                                                      "AllowedHosts": "*",
                                                      "AWS__Region": "eu-west-1",
                                                      "AWS__Profile": "sdcdev",
                                                      "AWS__DynamoDbServiceUrl": "http://localhost:8001",
                                                      "AwsCognitoSettings__Authority": "https://cognito-idp.eu-west-1.amazonaws.com/eu-west-1_WWflkHNwn",
                                                      "AwsCognitoSettings__Audience": "7oup5j6busol448gnor8lv0qi6",
                                                      "AwsStepFunctionSetting__DeployAwsStateMachineArn": "arn:aws:states:eu-west-1:783764573141:stateMachine:sdc-capability-rollout",
                                                      "OpenApiInfos__Versions__0__Audience": "company-internal",
                                                      "OpenApiInfos__Versions__0__ContactEmail": "philip.pregler@siemens.com",
                                                      "OpenApiInfos__Versions__0__ContactName": "Pulse Core API Team",
                                                      "OpenApiInfos__Versions__0__ContactUrl": "https://www.pulse.de",
                                                      "OpenApiInfos__Versions__0__Description": "API which provides all core functions",
                                                      "OpenApiInfos__Versions__0__Id": "5064d915-f010-4223-956f-8f18e3ac43d0",
                                                      "OpenApiInfos__Versions__0__Title": "API V1 - In Test"
                                                    }

                                                    """;

        public async Task WriteFileStructureAsync(SolutionFile solutionFile,
                                                  ProjectFile clientProject,
                                                  ProjectFile clientTestProject,
                                                  string projectName,
                                                  string clientName)
        {
            // 1. Environment folder
            // Test structure:
            // Environment
            // -> MsTestBase
            var environmentFolder = new DirectoryInfo(Path.Combine(clientTestProject.ProjectFileInfo.Value.Directory!.FullName, "Environment"));

            if (environmentFolder.NotExists())
            {
                environmentFolder.Create();
            }

            // 2. MsTest base class
            var msTestBaseClass = msTestBaseClassBuilder.BuildFor(clientTestProject.ProjectFileInfo.Value.NameWithoutExtension(), clientName);
            var msTestBaseFile = new FileInfo(Path.Combine(environmentFolder.FullName, "ApiTestBase.cs"));

            // Quickfix Startup vs Program.cs
            // If on any csproj root level is no startup so we have program.cs only
            var programFile = solutionFile.ProductiveProjects.Where(p => p.ProjectFileInfo.FileNameWithoutExtenion.EqualsTo(solutionFile.SolutionFileInfo.FileNameWithoutExtenion)).SelectMany(p => p.CSharpFileInfos).Where(c => c.Value.Name.Contains("program.cs", StringComparison.OrdinalIgnoreCase)).ToList();
            var startup = programFile.SelectMany(p => p.Value.Directory!.EnumerateFiles("startup.cs", SearchOption.TopDirectoryOnly));

            if (startup.IsEmpty())
            {
                msTestBaseClass = msTestBaseClass.Replace("<Startup>", "<Program>");
            }

            // 2.1 Important we will not overwrite if it exists already because of custom changes
            if (msTestBaseFile.NotExists())
            {
                await File.WriteAllTextAsync(msTestBaseFile.FullName, msTestBaseClass).ConfigureAwait(false);
            }

            // 3. appsettings.test.json
            var appSettings = appsettingsBuilder.BuildFor(projectName, clientName);
            var appSettingsFile = new FileInfo(Path.Combine(clientTestProject.ProjectFileInfo.Value.Directory!.FullName, "appsettings.test.json"));

            // 3.1 Important we will not overwrite if it exists already because of custom changes
            if (appSettingsFile.NotExists())
            {
                await File.WriteAllTextAsync(appSettingsFile.FullName, appSettings).ConfigureAwait(false);
            }

            // NEW Jsonserializer
            var jsonSerializer = jsonSerializerBuilder.BuildFor(projectName, clientName);
            var jsonSerializerFile = new FileInfo(Path.Combine(clientProject.ProjectFileInfo.Value.Directory!.FullName, "Serializer", "JsonSerializer.cs"));

            if (jsonSerializerFile.Directory!.NotExists())
            {
                jsonSerializerFile.Directory!.Create();
            }

            await File.WriteAllTextAsync(jsonSerializerFile.FullName, jsonSerializer).ConfigureAwait(false);

            // NEW health endpoint test
            var healthTest = new FileInfo(Path.Combine(clientTestProject.ProjectFileInfo.Value.Directory!.FullName, "Api", "Health",
                                                       "GetHealthStateTest.cs"));

            if (healthTest.Directory!.NotExists())
            {
                healthTest.Directory!.Create();
            }

            var healthTestSyntaxTree = HealthTestTemplate.Replace("$name$", clientName)
                                                         .Replace("$clientName$", clientName)
                                                         .Replace("$namespace$", clientTestProject.ProjectFileInfo.FileNameWithoutExtenion);

            await File.WriteAllTextAsync(healthTest.FullName, healthTestSyntaxTree);

            // Healthy response
            var healthyResponseFile = new FileInfo(Path.Combine(clientTestProject.ProjectFileInfo.Value.Directory!.FullName, "Api", "Health",
                                                                "Responses", "Healthy.json"));

            if (healthyResponseFile.Directory!.NotExists())
            {
                healthyResponseFile.Directory!.Create();
            }

            await File.WriteAllTextAsync(healthyResponseFile.FullName, HealthResponseFile);

            // Environment variables
            var environmenVariables = new FileInfo(Path.Combine(clientTestProject.ProjectFileInfo.Value.Directory!.FullName, "Properties", "EnvironmentVariables.json"));

            if (environmenVariables.Directory!.NotExists())
            {
                environmenVariables.Directory!.Create();
            }

            await File.WriteAllTextAsync(environmenVariables.FullName, EnvironmentVariables);

            // 4. Add project references
            var dotNetTool = DotNetToolFactory.Create();

            // 4.1 Client project reference is needed
            await dotNetTool.RunAsync("dotnet", $"add {clientTestProject.ProjectFileInfo.Value.FullName} reference {clientProject.ProjectFileInfo.Value.FullName}").ConfigureAwait(false);

            // 4.2 API project reference is needed too because of startup.cs
            var webAppProject = solutionFile.ProductiveProjects.FirstOrDefault(p => p.Document.ToString().Contains("Sdk=\"Microsoft.NET.Sdk.Web\""));

            if (webAppProject.IsNotNull())
            {
                await dotNetTool.RunAsync("dotnet", $"add {clientTestProject.ProjectFileInfo.Value.FullName} reference {webAppProject.ProjectFileInfo.Value.FullName}").ConfigureAwait(false);
            }
        }
    }
}
