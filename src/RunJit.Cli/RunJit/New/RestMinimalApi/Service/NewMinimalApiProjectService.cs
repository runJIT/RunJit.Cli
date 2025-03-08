using System;
using System.Collections.Generic;
using System.Text;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using PluralizeService.Core;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.New.MinimalApiProject;
using RunJit.Cli.RunJit.New.RestMinimalApi.Service;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;
using Solution.Parser.Solution;
using CSharpSyntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree;

namespace RunJit.Cli.New.RestMinimalApi
{
    internal sealed record CreateRestApiInfos()
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

            services.AddSingletonIfNotExists<NewRestMinimalApiService>();
        }
    }

    internal sealed class NewRestMinimalApiService(ConsoleService consoleService,
                                                   GenerateMigrationScript generateMigrationScript,
                                                   IEnumerable<IRestMinimalApiSpecificCodeGen> codeGenerators,
                                                   IEnumerable<IRestMinimalApiTestSpecificCodeGen> testCodeGenerators)
    {
        public async Task<int> HandleAsync(NewRestMinimalApiParameters parameters)
        {
            if (parameters.SolutionFile.IsNotNull() &&
                parameters.SolutionFile.NotExists())
            {
                throw new RunJitException($"Passed solution file: {parameters.SolutionFile.FullName} does not exists");
            }


            if (parameters.SolutionFile.IsNull() &&
                parameters.GitRepos.IsNullOrWhiteSpace())
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

            var queryPropertyName = record.Properties.FirstOrDefault(p => p.Name == parameters.QueryProperty);

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


            var properties = record.Properties;
            var propertiesWithoutId = properties.Where(p => p.Name.NotEqualsTo(hashKeyPropertyId.Name)).Select(p => p.SyntaxTree.Split(Environment.NewLine).Last()).Flatten($"{Environment.NewLine}");
            var allPropertiesNeutral = properties.Select(p => p.SyntaxTree.Split(Environment.NewLine).Last()).Flatten($"{Environment.NewLine}");
            var domainModel = $@"public record {record.Name.Replace("Entity", string.Empty)}                                 
                                {{
                                {allPropertiesNeutral}
                                }}
                                ".FormatSyntaxTree();


            var domainNamePlural = PluralizationProvider.Pluralize(parameters.DomainName);
            var domainName = PluralizationProvider.Singularize(parameters.DomainName);

            var propertyMapping = properties.Where(p => p.Name.NotEqualsTo(hashKeyPropertyId.Name)).Select(property => $"{property.Name} = source.{property.Name},").Flatten(Environment.NewLine);



            var parsedClientSolution = new SolutionFileInfo(parameters.SolutionFile.FullName).Parse();

            var programFile = parsedClientSolution.ProductiveProjects.FirstOrDefault(p =>
                                                                                     {
                                                                                         var program = p.CSharpFileInfos.FirstOrDefault(f => f.Value.NameWithoutExtension() == "Program");

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


            var testPayloadJson = properties.Where(p => p.Name.NotEqualsTo(hashKeyPropertyId.Name))
                                            .ToDictionary(item => item.Name, item =>
                                                                             {
                                                                                 if (item.Name == queryPropertyName.Name)
                                                                                 {
                                                                                     return "$Unique$DomainName$Name$";
                                                                                 }
                                                                                 return item.Name;
                                                                             })
                                            .ToJsonIntended();

            var testResponseJson = properties.ToDictionary(item => item.Name, item =>
                                                                                {
                                                                                    if (item.Name == hashKeyPropertyId.Name)
                                                                                    {
                                                                                        return Guid.NewGuid().ToString();
                                                                                    }

                                                                                    if (item.Name == queryPropertyName.Name)
                                                                                    {
                                                                                        return "$Unique$DomainName$Name$";
                                                                                    }
                                                                                    return item.Name;
                                                                                })
                                             .ToJsonIntended();


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
                PropertiesWithoutId = propertiesWithoutId,
                IdPropertyName = hashKeyPropertyId.Name,
                QueryPropertyName = parameters.QueryProperty,
                QueryPropertyNameLower = parameters.QueryProperty.FirstCharToLower(),
                MigrationScript = migrationScript,
                TestRequestJson = testPayloadJson,
                TestResponseJson = testResponseJson
            };


            foreach (var restMinimalApiSpecificCodeGen in codeGenerators)
            {
                await restMinimalApiSpecificCodeGen.GenerateAsync(parameters.SolutionFile, programFile.ProjectFileInfo.Value, createRestApiInfos);
            }

            foreach (var restMinimalApiTestSpecificCodeGen in testCodeGenerators)
            {
                await restMinimalApiTestSpecificCodeGen.GenerateAsync(parameters.SolutionFile, testProject.ProjectFileInfo.Value, createRestApiInfos);
            }

            // 3. Write success message
            consoleService.WriteSuccess($"Enjoy your new rest api endpoint :)");

            return 0;
        }
    }


}
