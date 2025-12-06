using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using PluralizeService.Core;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.New.MinimalApiProject;
using RunJit.Cli.RunJit.New.RestMinimalApi.CodeBuilders;
using RunJit.Cli.RunJit.New.RestMinimalApi.Service;
using RunJit.Cli.Services;
using RunJit.Cli.Services.Resharper;
using Solution.Parser.CSharp;
using Solution.Parser.Solution;
using Attribute = Solution.Parser.CSharp.Attribute;
using CSharpSyntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree;

namespace RunJit.Cli.New.RestMinimalApi
{
    internal sealed record CreateRestApiInfos
    {
        internal required string ProjectName { get; init; }

        internal required string DomainModelCode { get; init; }

        internal required string EntityModelCode { get; init; }

        internal required string DomainNameLower { get; init; }

        internal required string DomainName { get; init; }

        internal required string DomainNamePlural { get; init; }

        internal required string DomainNamePluralLower { get; init; }

        internal required string PropertyMappings { get; init; }

        internal required string PropertiesWithoutId { get; init; }

        internal required string IdPropertyName { get; init; }

        internal required string QueryPropertyName { get; init; }

        internal required string QueryPropertyNameLower { get; init; }

        internal required string MigrationScript { get; init; }

        internal required string TestRequestJson { get; init; }

        internal required string TestResponseJson { get; init; }

        internal required int Version { get; init; }

        internal required string BasePath { get; init; }

        // NEW WIP validation checks
        internal string CreateRequestValidations { get; init; } = string.Empty;

        internal string DeleteAllRequestValidations { get; init; } = string.Empty;

        internal string DeleteByIdRequestValidations { get; init; } = string.Empty;

        internal string GetAllRequestValidations { get; init; } = string.Empty;

        internal string GetByIdRequestValidations { get; init; } = string.Empty;

        internal string PatchRequestValidations { get; init; } = string.Empty;

        internal string UpdateRequestValidations { get; init; } = string.Empty;
    }

    internal interface IRestMinimalApiSpecificCodeGen
    {
        Task GenerateAsync(FileInfo solutionFileInfo,
                           FileInfo webApiProject,
                           CreateRestApiInfos createRestApiInfos);
    }

    internal interface IRestMinimalApiTestSpecificCodeGen
    {
        Task GenerateAsync(FileInfo solutionFileInfo,
                           FileInfo webApiProject,
                           CreateRestApiInfos createRestApiInfos);
    }

    internal static class AddNewRestMinimalApiServiceExtension
    {
        internal static void AddNewRestMinimalApiService(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddProcessService();
            services.AddMinimalApiProjectCreator();
            services.AddWriteEmbbededFileIntoTarget();
            services.AddStartupRegistration();
            services.AddStartupRegistrationVersions();
            services.AddGenerateMigrationScript();
            services.AddApiNamespaceProviderCleanup();
            services.AddDatabaseNamespaceProviderCleanup();
            services.AddSimpleValidationCodeBuilder();
            ;

            services.AddSingletonIfNotExists<NewRestMinimalApiService>();
        }
    }

    internal sealed class NewRestMinimalApiService(ConsoleService consoleService,
                                                   GenerateMigrationScript generateMigrationScript,
                                                   IEnumerable<IRestMinimalApiSpecificCodeGen> codeGenerators,
                                                   IEnumerable<IRestMinimalApiTestSpecificCodeGen> testCodeGenerators,
                                                   SolutionCodeCleanup solutionCodeCleanup,
                                                   SimpleValidationCodeBuilder simpleValidationCodeBuilder)
    {
        public async Task<int> HandleAsync(NewRestMinimalApiParameters parameters)
        {
            if (parameters.SolutionFilesOrGitRepos.IsNullOrWhiteSpace())
            {
                throw new RunJitException($@"Your passed {nameof(NewRestMinimalApiParameters.SolutionFilesOrGitRepos)} is null, empty or whitespace. Please pass your absolute path to your solution file (sample: D:\\Siemens\\siemens-aspnet-errorhandler\\Siemens.AspNet.ErrorHandler.sln\\) or git repository urls (sample: 'https://github.siemens.cloud/sdc/siemens-aspnet-errorhandler.git' or multiple 'codecommit::eu-central-1://pulse-datamanagement;https://github.siemens.cloud/sdc/siemens-aspnet-errorhandler.git' separated by ';'");
            }

            // Check if it is a solution file
            var splittedValues = parameters.SolutionFilesOrGitRepos.Split(";", StringSplitOptions.RemoveEmptyEntries);
            var solutionFiles = splittedValues.Where(value => value.EndsWith(".sln")).Select(f => new FileInfo(f)).ToList();
            var notExistingFiles = solutionFiles.Where(f => f.NotExists()).ToList();

            if (notExistingFiles.Any())
            {
                throw new RunJitException($@"Your passed solution files does not exist:{Environment.NewLine}{notExistingFiles.Select(f => f.FullName).Flatten(Environment.NewLine)}");
            }

            if (parameters.SolutionFilesOrGitRepos.IsNull() &&
                parameters.SolutionFilesOrGitRepos.IsNullOrWhiteSpace())
            {
                throw new RunJitException($"You have to pass at least the solution file ''--solution D:\\My.sln'' or a git repo url '--git-repos codecommit::eu-central-1://runjit-dbi' that this command can work");
            }

            if (parameters.Version < 1)
            {
                throw new RunJitException($"The api version must be greater than 0. It can be 1 but not smaller");
            }

            if (parameters.QueryProperty.IsNullOrWhiteSpace())
            {
                throw new RunJitException($"Query property name must not be null, empty or whitespace");
            }

            var syntaxTree = parameters.DbEntityModel.EndsWith(".cs") ? CSharpSyntaxTree.ParseText(await File.ReadAllTextAsync(parameters.DbEntityModel)) : CSharpSyntaxTree.ParseText(parameters.DbEntityModel);
            var simplifiedSyntaxTree = syntaxTree.Parse(string.Empty);

            if (simplifiedSyntaxTree.Records.IsEmpty())
            {
                var sample = """
                             [DynamoDBTable("Project")]
                             public record ProjectEntity
                             {
                                 [DynamoDBHashKey]
                                 public Guid ProjectId { get; init; } = Guid.Empty;

                                 public string Name { get; init; } = string.Empty;

                                 public string Description { get; init; } = string.Empty;
                             }
                             """;

                throw new RunJitException($"The passed domain model must be a record type in c#. Sample: {Environment.NewLine}{sample}");
            }

            if (simplifiedSyntaxTree.Records.Count > 1)
            {
                var sample = """
                             [DynamoDBTable("Project")]
                             public record ProjectEntity
                             {
                                 [DynamoDBHashKey]
                                 public Guid ProjectId { get; init; } = Guid.Empty;

                                 public string Name { get; init; } = string.Empty;

                                 public string Description { get; init; } = string.Empty;
                             }
                             """;

                throw new RunJitException($"Please support only 1 simple record declaration in C#. At the moment we are not supporting multiple entities at once. Sample: {Environment.NewLine}{sample}");
            }

            var record = simplifiedSyntaxTree.Records.First();

            var queryPropertyName = record.Properties.FirstOrDefault(p => p.Name.EqualsTo(parameters.QueryProperty));

            if (queryPropertyName.IsNull())
            {
                throw new RunJitException($"Your passed query property name: {parameters.QueryProperty} does not exists on your passed entity model:{Environment.NewLine}{syntaxTree}");
            }

            if (record.Attributes.Any(a => a.Name.Contains("DynamoDBTable").IsFalse()))
            {
                var sample = """
                             [DynamoDBTable("Project")]
                             public record ProjectEntity
                             {
                                 [DynamoDBHashKey]
                                 public Guid ProjectId { get; init; } = Guid.Empty;

                                 public string Name { get; init; } = string.Empty;

                                 public string Description { get; init; } = string.Empty;
                             }
                             """;

                throw new RunJitException($"Your provided record type does not have a mandatory [DynamoDBTable(\"Project\")] attribute. Sample: {Environment.NewLine}{sample}");
            }

            var hashKeyPropertyId = record.Properties.FirstOrDefault(p => p.SyntaxTree.Contains("DynamoDBHashKey"));

            if (hashKeyPropertyId.IsNull())
            {
                var sample = """
                             [DynamoDBTable("Project")]
                             public record ProjectEntity
                             {
                                 [DynamoDBHashKey]
                                 public Guid ProjectId { get; init; } = Guid.Empty;

                                 public string Name { get; init; } = string.Empty;

                                 public string Description { get; init; } = string.Empty;
                             }
                             """;

                throw new RunJitException($"Your provided record type does not have a mandatory [DynamoDBHashKey] attribute. Sample: {Environment.NewLine}{sample}");
            }

            if (record.Name.EndsWith("Entity").IsFalse())
            {
                var sample = """
                             [DynamoDBTable("Project")]
                             public record ProjectEntity
                             {
                                 [DynamoDBHashKey]
                                 public Guid ProjectId { get; init; } = Guid.Empty;

                                 public string Name { get; init; } = string.Empty;

                                 public string Description { get; init; } = string.Empty;
                             }
                             """;

                throw new RunJitException($"Your provided record type does not have the correct post fix 'Entity'. Sample: {Environment.NewLine}{sample}");
            }

            var queryProperties = new Property("string",
                                               parameters.QueryProperty,
                                               true,
                                               true,
                                               false,
                                               Modifier.Public.AsImmutableList(),
                                               ImmutableList<Attribute>.Empty,
                                               $"public string {parameters.QueryProperty} {{ get; init; }}",
                                               string.Empty,
                                               string.Empty).ToIList();

            var properties = record.Properties;
            var propertiesWithoutId = properties.Where(p => p.Name.NotEqualsTo(hashKeyPropertyId.Name)).ToList();
            var propertiesWithoutIdAsString = propertiesWithoutId.Select(p => p.SyntaxTree.Split(Environment.NewLine).Last()).Flatten($"{Environment.NewLine}");
            var allPropertiesNeutral = properties.Select(p => p.SyntaxTree.Split(Environment.NewLine).Last()).Flatten($"{Environment.NewLine}");

            var domainModel = $@"public record {record.Name.Replace("Entity", string.Empty)}
                                {{
                                {allPropertiesNeutral}
                                }}
                                ".FormatSyntaxTree();

            var domainNamePlural = PluralizationProvider.Pluralize(parameters.DomainName);
            var domainName = PluralizationProvider.Singularize(parameters.DomainName);

            var propertyMapping = propertiesWithoutId.Select(property => $"{property.Name} = source.{property.Name},").Flatten(Environment.NewLine);

            // For each solution file and git repo
            // we integrate the new apis
            foreach (var splittedValue in splittedValues)
            {
                if (splittedValue.EndsWith(".sln").IsFalse())
                {
                    consoleService.WriteError($@"We are currently support only solution files to add new web apis. Your passed value: {splittedValue} is not a valid solution file path. Sample: D:\Siemens\siemens-aspnet-errorhandler\Siemens.AspNet.ErrorHandler.sln");

                    continue;
                }

                var solutionFileInfo = new FileInfo(splittedValue);

                if (solutionFileInfo.NotExists())
                {
                    consoleService.WriteError($@"Your passed solution file does not exist: {solutionFileInfo.FullName}");

                    continue;
                }

                var parsedClientSolution = new SolutionFileInfo(solutionFileInfo.FullName).Parse();

                var programFile = parsedClientSolution.ProductiveProjects.FirstOrDefault(p =>
                                                                                         {
                                                                                             var program = p.CSharpFileInfos.FirstOrDefault(f => f.Value.NameWithoutExtension().EqualsTo("Program"));

                                                                                             if (program.IsNotNull())
                                                                                             {
                                                                                                 var text = File.ReadAllText(program.Value.FullName);

                                                                                                 if (text.Contains("new ServerlessMinimalWebApi();"))
                                                                                                 {
                                                                                                     return true;
                                                                                                 }
                                                                                             }

                                                                                             return false;
                                                                                         });

                if (programFile.IsNull())
                {
                    throw new RunJitException("Cant find a project files which is using ServerlessMinimalWebApi(). This new REST-API gen is only made for this new type of web api projects");
                }

                var testProject = parsedClientSolution.UnitTestProjects.FirstOrDefault(p => p.ProjectFileInfo.FileNameWithoutExtenion.StartsWith($"{programFile.ProjectFileInfo.FileNameWithoutExtenion}.Test"));

                if (testProject.IsNull())
                {
                    throw new RunJitException($"Cant find the test project for the web api. Please check the naming. Expected: {programFile}.Test.csproj");
                }

                var migrationScript = generateMigrationScript.Generate(record);

                var testPayloadJson = propertiesWithoutId
                                      .ToDictionary(item => item.Name, item =>
                                                                       {
                                                                           if (item.Name.EqualsTo(queryPropertyName.Name))
                                                                           {
                                                                               return $"$Unique{domainName}Name$";
                                                                           }

                                                                           return item.Name;
                                                                       })
                                      .ToJsonIntended();

                var testResponseAsJson = properties.ToDictionary(item => item.Name, item =>
                                                                                    {
                                                                                        if (item.Name.EqualsTo(hashKeyPropertyId.Name))
                                                                                        {
                                                                                            return Guid.NewGuid().ToString();
                                                                                        }

                                                                                        if (item.Name.EqualsTo(queryPropertyName.Name))
                                                                                        {
                                                                                            return $"$Unique{domainName}Name$";
                                                                                        }

                                                                                        return item.Name;
                                                                                    }).ToJsonIntended();

                var createRestApiInfos = new CreateRestApiInfos
                {
                    Version = parameters.Version,
                    DomainModelCode = domainModel,
                    EntityModelCode = record.SyntaxTree,
                    DomainName = domainName,
                    DomainNameLower = domainName.FirstCharToLower(),
                    DomainNamePlural = domainNamePlural,
                    DomainNamePluralLower = domainNamePlural.FirstCharToLower(),
                    PropertyMappings = propertyMapping,
                    ProjectName = programFile.ProjectFileInfo.FileNameWithoutExtenion,
                    PropertiesWithoutId = propertiesWithoutIdAsString,
                    IdPropertyName = hashKeyPropertyId.Name,
                    QueryPropertyName = parameters.QueryProperty,
                    QueryPropertyNameLower = parameters.QueryProperty.FirstCharToLower(),
                    MigrationScript = migrationScript,
                    TestRequestJson = testPayloadJson,
                    TestResponseJson = testResponseAsJson,
                    BasePath = parameters.BasePath
                };

                var createRequestValidation = simpleValidationCodeBuilder.BuildSimpleValidations(propertiesWithoutId, "Create", createRestApiInfos);
                var deleteAllRequestValidation = simpleValidationCodeBuilder.BuildSimpleValidations(queryProperties, "Delete", createRestApiInfos);
                var deleteByIdRequestValidation = simpleValidationCodeBuilder.BuildSimpleValidations([hashKeyPropertyId], "DeleteById", createRestApiInfos);
                var getAllRequestValidation = simpleValidationCodeBuilder.BuildSimpleValidations(queryProperties, "Get", createRestApiInfos);
                var getByIdRequestValidation = simpleValidationCodeBuilder.BuildSimpleValidations([hashKeyPropertyId], "GetById", createRestApiInfos);
                var patchRequestValidation = simpleValidationCodeBuilder.BuildSimpleValidations(propertiesWithoutId, "Patch", createRestApiInfos);
                var udpateRequestValidation = simpleValidationCodeBuilder.BuildSimpleValidations(propertiesWithoutId, "Update", createRestApiInfos);

                createRestApiInfos = createRestApiInfos with
                {
                    CreateRequestValidations = createRequestValidation,
                    DeleteAllRequestValidations = deleteAllRequestValidation,
                    DeleteByIdRequestValidations = deleteByIdRequestValidation,
                    GetAllRequestValidations = getAllRequestValidation,
                    GetByIdRequestValidations = getByIdRequestValidation,
                    PatchRequestValidations = patchRequestValidation,
                    UpdateRequestValidations = udpateRequestValidation
                };

                foreach (var restMinimalApiSpecificCodeGen in codeGenerators)
                {
                    await restMinimalApiSpecificCodeGen.GenerateAsync(solutionFileInfo, programFile.ProjectFileInfo.Value, createRestApiInfos);
                }

                foreach (var restMinimalApiTestSpecificCodeGen in testCodeGenerators)
                {
                    await restMinimalApiTestSpecificCodeGen.GenerateAsync(solutionFileInfo, testProject.ProjectFileInfo.Value, createRestApiInfos);
                }

                await solutionCodeCleanup.CleanupSolutionAsync(solutionFileInfo).ConfigureAwait(false);

                // 3. Write success message
                consoleService.WriteSuccess($"Enjoy your new rest api endpoint :)");
            }

            return 0;
        }
    }
}
