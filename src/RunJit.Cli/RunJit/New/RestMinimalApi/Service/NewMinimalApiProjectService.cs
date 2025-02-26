using Extensions.Pack;
using Microsoft.Build.Construction;
using Microsoft.Extensions.DependencyInjection;
using PluralizeService.Core;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.New.MinimalApiProject;
using RunJit.Cli.Services;
using Solution.Parser.CSharp;
using Solution.Parser.Solution;
using CSharpSyntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree;

namespace RunJit.Cli.New.RestMinimalApi
{
    internal static class AddNewRestMinimalApiServiceExtension
    {
        internal static void AddNewRestMinimalApiService(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddProcessService();
            services.AddMinimalApiProjectCreator();
            services.AddWriteEmbbededFileIntoTarget();

            services.AddSingletonIfNotExists<NewRestMinimalApiService>();
        }
    }

    internal sealed class NewRestMinimalApiService(ConsoleService consoleService,
                                                   IEnumerable<IRestMinimalApiSpecificCodeGen> codeGenerators)
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


            var syntaxTree = CSharpSyntaxTree.ParseText(parameters.DomainModel);
            var simplifiedSyntaxTree = syntaxTree.Parse(string.Empty);


            if (simplifiedSyntaxTree.Records.IsEmpty() &&
                simplifiedSyntaxTree.Classes.IsEmpty())
            {

                var sample = """
                             public record User
                             {
                                 public Guid Id { get; init; } = Guid.Empty;
                             
                                 public string Name { get; init; } = string.Empty;
                             
                                 public string Description { get; init; } = string.Empty;
                             }
                             """;

                throw new RunJitException($"The passed domain model does not have a class or a record please pass a valid c# class or record syntax. Sample: {Environment.NewLine}{sample}");
            }

            var record = simplifiedSyntaxTree.Records.FirstOrDefault();
            var @class = simplifiedSyntaxTree.Classes.FirstOrDefault();

            var domainName = record.IsNotNull() ? record.Name.FirstCharToUpper() : @class!.Name.FirstCharToUpper();

            var domainNamePlural = PluralizationProvider.Pluralize(domainName);
            var properties = record.IsNotNull() ? record.Properties : @class!.Properties;
            var propertyMapping = properties.Select(property => $"{property.Name} = source.{property.Name},").Flatten(Environment.NewLine);


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

            var createRestApiInfos = new CreateRestApiInfos
            {
                Version = parameters.Version,
                DomainModelSyntaxTree = simplifiedSyntaxTree,
                DomainModelCode = parameters.DomainModel,
                DomainName = domainName,
                DomainNameLower = domainName.FirstCharToLower(),
                DomainNamePlural = domainNamePlural,
                DomainNamePluralLower = domainNamePlural.FirstCharToLower(),
                PropertyMappings = propertyMapping,
                ProjectName = programFile.ProjectFileInfo.FileNameWithoutExtenion
            };


            foreach (var restMinimalApiSpecificCodeGen in codeGenerators)
            {
                await restMinimalApiSpecificCodeGen.GenerateAsync(parameters.SolutionFile, programFile.ProjectFileInfo.Value, createRestApiInfos);
            }

            // 3. Write success message
            consoleService.WriteSuccess($"Enjoy your new rest api endpoint :)");

            return 0;
        }
    }
}
