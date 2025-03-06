using System.Runtime.CompilerServices;
using $ProjectName$.Helpers;


namespace $ProjectName$.Extensions
{
    internal static class EndpointConventionBuilderExtensions
    {
        private static readonly EmbeddedFileLocalizer EmbeddedFileLocalizer = new EmbeddedFileLocalizer();
            
        internal static TBuilder WithSummaryFromFile<TBuilder>(this TBuilder builder,
                                                               string embeddedFile,
                                                               [CallerFilePath] string callerFilePath = "")
            where TBuilder : IEndpointConventionBuilder
        {
            var fileContent = EmbeddedFileLocalizer.LocalizeDocumentation(embeddedFile, callerFilePath);

            return builder.WithSummary(fileContent);
        }

        internal static TBuilder WithDescriptionFromFile<TBuilder>(this TBuilder builder,
                                                                   string embeddedFile,
                                                                   [CallerFilePath] string callerFilePath = "")
            where TBuilder : IEndpointConventionBuilder
        {
            var fileContent = EmbeddedFileLocalizer.LocalizeDocumentation(embeddedFile, callerFilePath);

            return builder.WithDescription(fileContent);
        }
    }
}

