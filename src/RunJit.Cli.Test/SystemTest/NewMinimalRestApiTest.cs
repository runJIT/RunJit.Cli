using System.Diagnostics;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RunJit.Cli.Test.Extensions;
using static RunJit.Cli.Test.SystemTest.NewServerlessMinimalApiTest;

namespace RunJit.Cli.Test.SystemTest
{
    // I need a record Project which have Id, Name and Description as property declaration
    public record Project
    {
        public Guid Id { get; init; } = Guid.Empty;

        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;
    }

    [TestCategory("runjit new minimal-api-serverless")]
    [TestClass]
    public class NewMinimalRestApiTest : GlobalSetup
    {
        // Source gen für migration script
        //
        //      aws dynamodb create-table \
        //         --table-name Project \
        //         --attribute-definitions AttributeName=ProjectId,AttributeType=S \
        //         --key-schema AttributeName=ProjectId,KeyType=HASH \
        //         --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
        //         --endpoint-url http://localhost:8001

        private const string ProjectEntityModel = """
                                                  [DynamoDBTable("Project")]
                                                  public record ProjectEntity
                                                  {
                                                      [DynamoDBHashKey]
                                                      public Guid ProjectId { get; init; } = Guid.Empty;

                                                      public string Name { get; init; } = string.Empty;

                                                      public string Description { get; init; } = string.Empty;
                                                  }
                                                  """;

        private const string UserEntityModel = """
                                               [DynamoDBTable("User")]
                                               public record UserEntity
                                               {
                                                   [DynamoDBHashKey]
                                                   public Guid UserId { get; init; } = Guid.Empty;

                                                   public string Name { get; init; } = string.Empty;

                                                   public string Phone { get; init; } = string.Empty;

                                                   public string Hobby { get; init; } = string.Empty;

                                                   public string Car { get; init; } = string.Empty;

                                                   public string FavoriteColor { get; init; } = string.Empty;
                                               }
                                               """;

        private const string ProviderRole = """
                                            [DynamoDBTable("ProviderRole")]
                                            public sealed record ProviderRoleEntity
                                            {
                                                [DynamoDBHashKey]
                                                public required Guid Id { get; init; }

                                                public required string Name { get; init; }

                                                public required string Description { get; init; }

                                                public required Dictionary<string, object> Permission { get; init; }
                                            }
                                            """;

        // +----------------------+--------+---------------------------+------------------+----------------+-------------------+
        // | CapabilityType       | Active | LastModifiedAt            | User             | Status         | Information       |
        // +----------------------+--------+---------------------------+------------------+----------------+-------------------+
        // | AwsS3Bucket          | true   | 2025-06-04T14:22:11Z      | admin@system     | Available      | -                 |
        // | Ec2                  | false  | 2025-06-03T10:08:45Z      | ops.engineer     | NotAvailable   | NotImplemented    |
        // | SnowflakeWarehouse   | true   | 2025-06-01T16:55:30Z      | infra.manager    | Available      | -                 |
        // +----------------------+--------+---------------------------+------------------+----------------+-------------------+

        private const string CapabilityType = """
                                              [DynamoDBTable("CapabilityType")]
                                              public record CapabilityTypeEntity
                                              {
                                                  [DynamoDBHashKey]
                                                  public required string Type { get; init; }

                                                  public bool Available { get; init; }

                                                  public string User { get; init; }

                                                  public string Information { get; init; }

                                                  [DateTimeOffsetIsUtc]
                                                  public required DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

                                                  [DateTimeOffsetIsUtc]
                                                  public required DateTimeOffset LastModifiedAt { get; init; } = DateTimeOffset.UtcNow;
                                              }
                                              """;

        private const string Cap = """
                                   [DynamoDBTable("Capability")]
                                   public record CapabilityEntity
                                   {
                                       [DynamoDBHashKey]
                                       public required Guid Id { get; init; }

                                       [DynamoDBRangeKey]
                                       public required string VersionTag { get; init; } = "latest";

                                       public string Name { get; init; } = string.Empty;

                                       public required string Type { get; init; }

                                       public required string Environment { get; init; }

                                       public required string LastUpdatedByUser { get; init; }

                                       public required DateTime LastUpdatedDate { get; init; }

                                       public string HostService { get; init; } = string.Empty;
                                   }
                                   """;

        private const string ApiKey = """
                                      [DynamoDBTable("ApiKey")]
                                      public record CapabilityApiKeyEntity
                                      {
                                          [DynamoDBHashKey]
                                          public required string CapabilityId { get; init; }

                                          public string Name { get; init; } = string.Empty;
                                      }
                                      """;

        private const string Capability = """
                                          [DynamoDBTable("Capability")]
                                          public record CapabilityEntity
                                          {
                                              [DynamoDBHashKey]
                                              public required Guid Id { get; init; }

                                              [DynamoDBRangeKey]
                                              public required string VersionTag { get; init; } = "latest";

                                              public string Name { get; init; } = string.Empty;

                                              public required string Type { get; init; }

                                              public required string Environment { get; init; }

                                              public required string LastUpdatedByUser { get; init; }

                                              public required DateTime LastUpdatedDate { get; init; }

                                              public string HostService { get; init; } = string.Empty;
                                          }
                                          """;

        private const string S3BucketEntity = """
                                              [DynamoDBTable("Capability")]
                                              public record S3BucketEntity
                                              {
                                                  [DynamoDBHashKey]
                                                  public required Guid Id { get; init; }

                                                  [DynamoDBRangeKey]
                                                  public required string VersionTag { get; init; } = "latest";

                                                  public string Name { get; init; } = string.Empty;

                                                  public required string Type { get; init; }

                                                  public required string Environment { get; init; }

                                                  public required string LastUpdatedByUser { get; init; }

                                                  public required DateTime LastUpdatedDate { get; init; }

                                                  public string HostService { get; init; } = string.Empty;
                                              }
                                              """;

        private const string S3TableBuckets = """
                                              [DynamoDBTable("Capability")]
                                              public record S3TableBucketEntity
                                              {
                                                  [DynamoDBHashKey]
                                                  public required Guid Id { get; init; }

                                                  [DynamoDBRangeKey]
                                                  public required string VersionTag { get; init; } = "latest";

                                                  public string Name { get; init; } = string.Empty;

                                                  public required string Type { get; init; }

                                                  public required string Environment { get; init; }

                                                  public required string LastUpdatedByUser { get; init; }

                                                  public required DateTime LastUpdatedDate { get; init; }

                                                  public string HostService { get; init; } = string.Empty;
                                              }
                                              """;

        private const string SageMakerUnifiedStudios = """
                                                       [DynamoDBTable("Capability")]
                                                       public record SageMakerUnifiedStudioEntity
                                                       {
                                                           [DynamoDBHashKey]
                                                           public required Guid Id { get; init; }

                                                           [DynamoDBRangeKey]
                                                           public required string VersionTag { get; init; } = "latest";

                                                           public string Name { get; init; } = string.Empty;

                                                           public required string Type { get; init; }

                                                           public required string Environment { get; init; }

                                                           public required string LastUpdatedByUser { get; init; }

                                                           public required DateTime LastUpdatedDate { get; init; }

                                                           public string HostService { get; init; } = string.Empty;
                                                       }
                                                       """;

        private const string AnimalEntity = """
                                            [DynamoDBTable("Animal")]
                                            public record AnimalEntity
                                            {
                                                [DynamoDBHashKey]
                                                public required Guid Id { get; init; }

                                                public string Name { get; init; } = string.Empty;
                                            }
                                            """;

        private const string CatEntity = """
                                         [DynamoDBTable("Animal")]
                                         public record CatEntity
                                         {
                                             [DynamoDBHashKey]
                                             public required Guid Id { get; init; }

                                             public string Name { get; init; } = string.Empty;
                                         }
                                         """;

        private const string DogEntity = """
                                         [DynamoDBTable("Animal")]
                                         public record DogEntity
                                         {
                                             [DynamoDBHashKey]
                                             public required Guid Id { get; init; }

                                             public string Name { get; init; } = string.Empty;
                                         }
                                         """;

        private const string CapabilityEntity = """
                                                // TableName attribute
                                                [DynamoDBTable("Capability")]
                                                public record CapabilityEntity
                                                {
                                                    [DynamoDBHashKey]
                                                    public required Guid Id { get; init; }

                                                    public string Name { get; init; } = string.Empty;

                                                    public string Type { get; init; } = string.Empty;
                                                }
                                                """;

        private const string UserEntity = """
                                          [DynamoDBTable("User")]
                                          public sealed record UserEntity
                                          {
                                              [DynamoDBHashKey]
                                              public Guid UniqueUserId  { get; init; }
                                              public string ExternalId  { get; init; }
                                              public string LastName  { get; init; }
                                              public string FirstName  { get; init; }
                                              public string Title  { get; init; }
                                              public string Email  { get; init; }
                                              public string OrgCode  { get; init; }
                                              public string OrgName  { get; init; }
                                              public string OrgId  { get; init; }
                                              public string CostCenter  { get; init; }
                                              public string Are  { get; init; }
                                              public string Country  { get; init; }
                                              public string Statu  { get; init; }
                                              public string ProjectRole { get; init; }
                                              public string MarketPlaceRole { get; init; }
                                              public string Scope { get; init; }
                                          }
                                          """;

        [DataTestMethod]

        //[DataRow("Sdc.Core", "api/core", "Core", "Projects", "Name", ProjectEntityModel)]
        //[DataRow("Sdc.UserManagement", "api/usermanagement", "um", "Users", "Name", UserEntityModel)]
        [DataRow("Sdc.UserManagement", "api/usermanagement", "um",
                    "Users", "Email", UserEntity)]
        public async Task Should_Add_New_Rest_Api_Into_New_Solution(string projectName,
                                                                    string basePath,
                                                                    string toolName,
                                                                    string domainName,
                                                                    string queryPropertyName,
                                                                    string entityModel)
        {
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, projectName);

            // 1. Create new solution and projects
            var solutionFileInfo = await Mediator.SendAsync(new NewMinimalApiProject(projectName, basePath, targetDirectory)).ConfigureAwait(false);

            // 2. Add rest api
            await Mediator.SendAsync(new NewMinimalRestApi(solutionFileInfo.FullName, entityModel, queryPropertyName,
                                                           domainName, basePath));

            // 3. Assert that solution can be build and needed for client as well
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 3. Assert that solution can be tested
            // await DotNetTool.AssertRunAsync("dotnet", $"test {solutionFileInfo.FullName}").ConfigureAwait(false);
        }

        [DataTestMethod]
        [DataRow("Sdc.Core", "api/core", "Core",
                    "Projects", "Name", ProjectEntityModel)]
        [DataRow("Sdc.UserManagement", "api/core", "um",
                    "Users", "Name", UserEntityModel)]
        public async Task Should_Add_New_Rest_Api_Into_Solution_From_File(string projectName,
                                                                          string basePath,
                                                                          string toolName,
                                                                          string domainName,
                                                                          string queryPropertyName,
                                                                          string entityModel)
        {
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, projectName);

            // 1. Create new solution and projects
            var solutionFileInfo = await Mediator.SendAsync(new NewMinimalApiProject(projectName, basePath, targetDirectory)).ConfigureAwait(false);

            // 2. Simulate file path
            var fileInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, "TestFile", "Entity.cs"));

            if (fileInfo.Directory!.NotExists())
            {
                fileInfo.Directory!.Create();
            }

            await File.WriteAllTextAsync(fileInfo.FullName, entityModel);

            // 3. Add rest api
            await Mediator.SendAsync(new NewMinimalRestApi(solutionFileInfo.FullName, fileInfo.FullName, queryPropertyName,
                                                           domainName, basePath));

            // 4. Assert that solution can be build and needed for client as well
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 5. Assert that solution can be tested
            // await DotNetTool.AssertRunAsync("dotnet", $"test {solutionFileInfo.FullName}").ConfigureAwait(false);
        }

        // S3TableBuckets und SageMakerUnifiedStudio

        [DataTestMethod]
        [DataRow("Sdc.Core", "api/core", "Core",
                    "Name")]
        [DataRow("Sdc.UserManagement", "api/core", "um",
                    "Name")]
        public async Task Should_Be_Able_To_Create_Multiple_Domains(string projectName,
                                                                    string basePath,
                                                                    string toolName,
                                                                    string queryPropertyName)
        {
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, projectName);

            // 1. Create new solution and projects
            var solutionFileInfo = await Mediator.SendAsync(new NewMinimalApiProject(projectName, basePath, targetDirectory)).ConfigureAwait(false);

            // 2. Add project api
            await Mediator.SendAsync(new NewMinimalRestApi(solutionFileInfo.FullName, AnimalEntity, queryPropertyName,
                                                           "Animals", basePath));

            //await Mediator.SendAsync(new NewMinimalRestApi(solutionFileInfo.FullName, CatEntity, queryPropertyName, "Cats", basePath));

            //await Mediator.SendAsync(new NewMinimalRestApi(solutionFileInfo.FullName, DogEntity, queryPropertyName, "Dogs", basePath));

            // await Mediator.SendAsync(new NewMinimalRestApi(solutionFileInfo.FullName, SageMakerUnifiedStudios, queryPropertyName, "SageMakerUnifiedStudios", basePath));

            // 4. Assert that solution can be build and needed for client as well
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 3. Assert that solution can be tested
            // await DotNetTool.AssertRunAsync("dotnet", $"test {solutionFileInfo.FullName}").ConfigureAwait(false);
        }

        [DataTestMethod]
        [DataRow("Sdc.Core", "api/core", "Core",
                    "Name")]
        [DataRow("Sdc.UserManagement", "api/core", "um",
                    "Name")]
        public async Task Should_Be_Able_To_Create_Same_Domain_In_Different_Versions(string projectName,
                                                                                     string basePath,
                                                                                     string toolName,
                                                                                     string queryPropertyName)
        {
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, projectName);

            // 1. Create new solution and projects
            var solutionFileInfo = await Mediator.SendAsync(new NewMinimalApiProject(projectName, basePath, targetDirectory)).ConfigureAwait(false);

            // 2. Add project api
            await Mediator.SendAsync(new NewMinimalRestApi(solutionFileInfo.FullName, ProjectEntityModel, queryPropertyName,
                                                           "Projects", basePath, Version: 1));

            // 3. Add user api
            await Mediator.SendAsync(new NewMinimalRestApi(solutionFileInfo.FullName, ProjectEntityModel, queryPropertyName,
                                                           "Projects", basePath, Version: 2));

            // 4. Assert that solution can be build and needed for client as well
            await DotNetTool.AssertRunAsync("dotnet", $"build {solutionFileInfo.FullName}").ConfigureAwait(false);

            // 3. Assert that solution can be tested
            // await DotNetTool.AssertRunAsync("dotnet", $"test {solutionFileInfo.FullName}").ConfigureAwait(false);
        }

        private const string FormsConfigurationEntity = """
                                                        [DynamoDBTable("FormsConfiguration")]
                                                        public sealed record FormsConfigurationEntity
                                                        {
                                                            [DynamoDBHashKey]
                                                            public required string FormsId { get; init; } // SDC --> FormsId GUID // Pulse --> SurveyInstanceId long

                                                            public required string ProjectId { get; init; } // SDC --> GUID // Pulse --> ProjectId long

                                                            public required string Title { get; init; } // unique runner id

                                                            public required FormsType FormsType { get; init; } // property??

                                                            public DateTime StartDate { get; init; } = DateTime.UtcNow;

                                                            public DateTime? EndDate { get; init; }

                                                            public long SessionTimeoutInSeconds { get; init; }

                                                            public bool HasUpdate { get; init; }

                                                            public List<Language> Languages { get; init; }

                                                            // public Contact? Contact { get; init; }

                                                            public Dictionary<string, object> Properties { get; init; } // HasInterviewExport = 1 // ContactInformation // SkipLandingPage //
                                                        }
                                                        """;

        [DataTestMethod]
        [DataRow(@"D:\Siemens\siemens-data-cloud-backend-console\Sdc.Console.sln", "api/console", "CapabilityApiKeys",
                    "Name", ApiKey)]
        public async Task Should_Add_New_Rest_Api_Into_Existing_Solution(string solutionFilePath,
                                                                         string basePath,
                                                                         string domainName,
                                                                         string queryPropertyName,
                                                                         string entityModel)
        {
            // 2. Add rest api
            await Mediator.SendAsync(new NewMinimalRestApi(solutionFilePath, entityModel, queryPropertyName,
                                                           domainName, basePath));
        }

        internal sealed record NewMinimalRestApi(string SolutionFileOrGitRepos,
                                                 string DbEntityModel,
                                                 string QueryProperty,
                                                 string DomainName,
                                                 string BasePath,
                                                 string GitRepos = "",
                                                 string WorkingDirectory = "",
                                                 int Version = 1,
                                                 string ExpectedErrorMessage = "") : ICommand<FileInfo>
        {
        }

        internal sealed class NewMinimalRestApiHandler : ICommandHandler<NewMinimalRestApi, FileInfo>
        {
            public async Task<FileInfo> Handle(NewMinimalRestApi request,
                                               CancellationToken cancellationToken)
            {
                await using var sw = new StringWriter();
                Console.SetOut(sw);

                var strings = CollectConsoleParameters(request).ToArray();
                var consoleCall = strings.Flatten(" ");
                Console.WriteLine();
                Console.WriteLine(consoleCall);
                Debug.WriteLine(consoleCall);
                var exitCode = await Program.Main(strings).ConfigureAwait(false);
                var output = sw.ToString();

                if (request.ExpectedErrorMessage.IsNotNullOrEmpty())
                {
                    Assert.AreEqual(1, exitCode);
                    Assert.IsTrue(output.Contains(request.ExpectedErrorMessage));
                }
                else
                {
                    Assert.AreEqual(0, exitCode, output);
                }

                // Last output must be the solution file
                var solutionFile = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Last();

                return new FileInfo(solutionFile);
            }

            private IEnumerable<string> CollectConsoleParameters(NewMinimalRestApi request)
            {
                yield return "runjit";
                yield return "new";
                yield return "minimal-rest-api";

                yield return request.SolutionFileOrGitRepos;

                if (request.WorkingDirectory.IsNotNullOrWhiteSpace())
                {
                    yield return "--working-directory";
                    yield return request.WorkingDirectory;
                }

                yield return "--base-path";
                yield return request.BasePath;

                yield return "--query-property";
                yield return request.QueryProperty;

                yield return "--version";
                yield return request.Version.ToInvariantString();

                yield return "--domain-name";
                yield return request.DomainName;

                yield return "--entity";
                yield return request.DbEntityModel;

                //yield return $""" "{request.DbEntityModel.Replace("\"", "\"\"")          // Escape double quotes
                //                           .Replace(Environment.NewLine, " ") // Replace Windows newlines with space
                //                           .Replace("\n", " ")                // Replace Unix newlines with space
                //                           .Replace("\r", " ")}" """;          // Just in case
            }
        }
    }
}
