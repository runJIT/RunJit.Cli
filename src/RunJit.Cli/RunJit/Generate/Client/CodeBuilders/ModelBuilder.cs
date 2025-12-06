using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Generate.Client
{
    internal static class AddModelBuilderExtension
    {
        internal static void AddModelBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ModelBuilder>();
        }
    }

    // What we are create here:
    // - We create classes which contains the copied data class from the api
    // - We only replace in the template RunJit.Generate.Client.Templates.model.rps the place holders with the expected values
    // Sample:
    //
    // using System;

    // namespace Api.Model.V1
    // {
    //     public record Person
    //     {
    //         public string Name { get; set; } = string.Empty;
    //     }
    // }
    public class ModelBuilder
    {
        private readonly string _modelTemplate = EmbeddedFile.GetFileContentFrom("RunJit.Generate.Client.Templates.model.rps");

        public string BuildFrom(DeclarationBase dataType,
                                GeneratedClientCodeForController controller,
                                string projectName,
                                string clientName)
        {
            var @namespace = controller.ControllerInfo.Version.IsNotNull() ? $"{projectName}.{ClientGenConstants.Api}.{controller.ControllerInfo.GroupName}.{controller.ControllerInfo.Version.Normalized}" : $"{projectName}.{ClientGenConstants.Api}.{controller.ControllerInfo.GroupName}";

            var model = _modelTemplate.Replace("$projectName$", projectName)
                                      .Replace("$clientName$", clientName)
                                      .Replace("$namespace$", @namespace)
                                      .Replace("$syntaxTree$", dataType.SyntaxTree);

            return model;
        }
    }
}
