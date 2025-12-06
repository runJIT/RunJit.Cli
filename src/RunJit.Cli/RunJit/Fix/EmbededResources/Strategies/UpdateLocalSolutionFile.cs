using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;
using RunJit.Cli.Services.Git;
using RunJit.Cli.Services.Net;
using Solution.Parser.Solution;

namespace RunJit.Cli.RunJit.Fix.EmbededResources
{
    internal static class AddUpdateLocalSolutionFileExtension
    {
        internal static void AddUpdateLocalSolutionFile(this IServiceCollection services)
        {
            services.AddConsoleService();
            services.AddGitService();
            services.AddDotNet();

            // services.AddFixEmbeddedResourcesPackageService();
            services.AddFindSolutionFile();

            services.AddSingletonIfNotExists<IFixEmbeddedResourcesStrategy, UpdateLocalSolutionFile>();
        }
    }

    internal sealed class UpdateLocalSolutionFile(FindSolutionFile findSolutionFile) : IFixEmbeddedResourcesStrategy
    {
        public bool CanHandle(FixEmbeddedResourcesParameters parameters)
        {
            return parameters.SolutionFile.IsNotNullOrWhiteSpace();
        }

        public Task HandleAsync(FixEmbeddedResourcesParameters parameters)
        {
            // 0. Check that precondition is met
            if (CanHandle(parameters).IsFalse())
            {
                throw new RunJitException($"Please call {nameof(IFixEmbeddedResourcesStrategy.CanHandle)} before call {nameof(IFixEmbeddedResourcesStrategy.HandleAsync)}");
            }

            // 1. Check if solution file is the file or directory
            //    if it is null or whitespace we check current directory
            // 5. Check if solution file is the file or directory
            //    if it is null or whitespace we check current directory 
            var solutionFile = findSolutionFile.Find(parameters.SolutionFile);

            var parsedSolutionFile = new SolutionFileInfo(solutionFile.FullName).Parse();
            var allCsprojFiles = parsedSolutionFile.Projects;

            foreach (var csprojFile in allCsprojFiles)
            {
                var csprojFileInfo = new FileInfo(csprojFile.ProjectFileInfo.Value.FullName);
                var csprojXml = XDocument.Load(csprojFileInfo.FullName);

                // Get all <EmbeddedResource Include="API\CalculateCo2\ExcelFiles\co2-sample-invalid.xlsx"> from csprojXml
                // and extract the file extension name
                var allEmbeddedFiles = csprojXml.Descendants().Where(e => e.Name.LocalName.EqualsTo("EmbeddedResource")).Select(node => node.Attribute("Include")?.Value).FilterNullObjects().ToList();
                var fileExtensions = allEmbeddedFiles.Select(file => file.Split('.').Last()).Distinct().Where(x => x.EndsWith("*").IsFalse()).ToList();

                var xmlElements = csprojXml.Descendants().Where(e => fileExtensions.Any(extension => e.Attributes().Any(a => a.Value.Contains($".{extension}") ||
                                                                                                                             a.Value.Contains(@"\*") ||
                                                                                                                             a.Value.Contains(@"\**")))).ToList();

                xmlElements.Remove();

                var itemGroup = new XElement("ItemGroup");

                foreach (var fileExtension in fileExtensions)
                {
                    var sqlElement = csprojXml.Descendants().FirstOrDefault(e => e.Name.LocalName.EqualsTo("EmbeddedResource") && (e.Attribute("Include")?.Value).EqualsTo($@"**\*.{fileExtension}"));

                    if (sqlElement.IsNull())
                    {
                        var embeddedResourceSql = new XElement("EmbeddedResource");
                        embeddedResourceSql.Add(new XAttribute("Include", $@"**\*.{fileExtension}"));

                        // Very important to exclude bin and obj folders
                        embeddedResourceSql.Add(new XAttribute("Exclude", @"bin\**\*;obj\**\*"));
                        itemGroup.Add(embeddedResourceSql);
                    }
                }

                if (itemGroup.HasElements)
                {
                    csprojXml.Root!.Add(itemGroup);
                }

                // all appsettings.x.json have to be ignored for now
                // all appsettings.x.json have to be ignored for now
                var appsettings = csprojFile.ProjectFileInfo.Value.Directory!.EnumerateFiles("appsetting*.json").ToList();

                if (appsettings.Any())
                {
                    var existAlready = csprojXml.Root!.ElementByAttribute("Include", "appsetting*.json");

                    if (existAlready.IsNull())
                    {
                        var itemgroupIgnore = new XElement("ItemGroup");
                        var appsettingElement = new XElement("EmbeddedResource");
                        appsettingElement.Add(new XAttribute("Remove", "appsetting*.json"));
                        itemgroupIgnore.Add(appsettingElement);

                        // Test project need special handling :/ 
                        if (appsettings.Any(a => a.Name.Contains("test")))
                        {
                            var copyToOutPut = new XElement("ItemGroup");
                            var content = new XElement("Content");
                            content.Add(new XAttribute("Include", "appsetting*.json"));
                            copyToOutPut.Add(content);
                            var copyToOutputDirectory = new XElement("CopyToOutputDirectory");
                            copyToOutputDirectory.Value = "PreserveNewest";
                            content.Add(copyToOutputDirectory);

                            csprojXml.Root!.Add(copyToOutPut);
                        }

                        csprojXml.Root!.Add(itemgroupIgnore);
                    }
                }

                // remove empty elements
                var empty = csprojXml.Descendants("ItemGroup").Where(e => e.HasElements.IsFalse()).ToList();
                empty.Remove();

                csprojXml.Save(csprojFileInfo.FullName);
            }

            return Task.CompletedTask;
        }
    }
}
