using System.Collections.Immutable;
using System.Net;
using Amazon;
using Amazon.Translate;
using Amazon.Translate.Model;
using Extensions.Pack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Localize.Strings
{
    internal static class AddStringLocalizerExtension
    {
        internal static void AddStringLocalizer(this IServiceCollection services)
        {
            services.AddExtractStringsToLocalize();

            services.AddSingletonIfNotExists<StringLocalizer>();
        }
    }

    internal sealed class StringLocalizer(ExtractStringsToLocalize extractStringsToLocalize)
    {
        // Hint: This is a prototype to check if we are able to do a full automation of the localization process.
        //
        //       This fixture allows you to localize all exception messages in all available language files.
        //       This code parse all the exception message strings out and create the keys and its default language "english" -> "en"
        //       into the language files.
        public async Task LocalizeAsync(ImmutableList<string> languages,
                                        string solutionPath)
        {
            // 1. Extract all declared strings in exceptions.
            //    Per default development mode is english !
            var projectsWithStringsToLocalize = await extractStringsToLocalize.ExtractLocalizableStrings(languages, solutionPath);

            // 2. POC use AWS translation service to translate the strings
            using var translationClient = new AmazonTranslateClient(RegionEndpoint.EUCentral1);

            // 3. Iterate over all projectsWithStringsToLocalize and then over all languages to translate the strings
            foreach (var projectWithStringsToLocalize in projectsWithStringsToLocalize)
            {
                foreach (var language in languages)
                {
                    if (projectWithStringsToLocalize.Project.FilePath.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    // Translation folder is per definition on the root level of the project
                    // We decide to have per project translation files per language
                    var translationFolder = new DirectoryInfo(Path.Combine(new FileInfo(projectWithStringsToLocalize.Project.FilePath).Directory!.FullName, "Translations"));
                    var languageFile = new FileInfo(Path.Combine(translationFolder.FullName, $"{language}.json"));

                    if (languageFile.Directory!.NotExists())
                    {
                        languageFile.Directory!.Create();
                    }

                    // Per default all text are english so no translation is needed
                    if (language.EqualsTo("en"))
                    {
                        await File.WriteAllTextAsync(languageFile.FullName, projectWithStringsToLocalize.TextToLocalize.ToJsonIntended());

                        continue;
                    }

                    // We are checking and loading the existing translations
                    // This is important not to translate already translated strings which have not changed
                    var existingTranslations = languageFile.Exists ? (await File.ReadAllTextAsync(languageFile.FullName)).FromJsonStringAs<Dictionary<string, string>>() : new Dictionary<string, string>();

                    var languageSpecificTranslations = new Dictionary<string, string>();

                    foreach (var textKeyAndText in projectWithStringsToLocalize.TextToLocalize)
                    {
                        if (textKeyAndText.Value.IsNullOrWhiteSpace())
                        {
                            languageSpecificTranslations[textKeyAndText.Key] = textKeyAndText.Value;

                            continue;
                        }

                        if (existingTranslations.TryGetValue(textKeyAndText.Key, out var translation))
                        {
                            languageSpecificTranslations[textKeyAndText.Key] = translation;
                        }
                        else
                        {
                            // ToDo: Text length < 1000 then split off
                            var translateRequest = new TranslateTextRequest
                                                   {
                                                       SourceLanguageCode = "en",
                                                       TargetLanguageCode = language,
                                                       Text = textKeyAndText.Value
                                                   };

                            // ToDo: Eval if language code is valid or AWS throws exception
                            var translatedTextResponse = await translationClient.TranslateTextAsync(translateRequest).ConfigureAwait(false);

                            if (translatedTextResponse.HttpStatusCode.NotEqualsTo(HttpStatusCode.OK))
                            {
                                continue;
                            }

                            languageSpecificTranslations[textKeyAndText.Key] = translatedTextResponse.TranslatedText;
                        }
                    }

                    await File.WriteAllTextAsync(languageFile.FullName, languageSpecificTranslations.ToJsonIntended());
                }
            }
        }

        private static IEnumerable<ThrowStatementSyntax> FindAllThrowStatements(SyntaxTree syntaxTree)
        {
            var root = syntaxTree.GetRoot();

            return root.DescendantNodes().OfType<ThrowStatementSyntax>()
                       .Where(ts => ts.Expression!.Is<ObjectCreationExpressionSyntax>());
        }

        private static MethodDeclarationSyntax? GetContainingMethod(SyntaxNode? node)
        {
            while (node != null)
            {
                if (node is MethodDeclarationSyntax methodDeclaration)
                {
                    return methodDeclaration;
                }

                node = node.Parent;
            }

            return null;
        }

        private static ClassDeclarationSyntax? GetContainingClass(SyntaxNode? node)
        {
            while (node != null)
            {
                if (node is ClassDeclarationSyntax classDeclaration)
                {
                    return classDeclaration;
                }

                node = node.Parent;
            }

            return null;
        }

        private static IEnumerable<TextToTranslate> GetTranslatableStrings(SemanticModel semanticModel,
                                                                           ThrowStatementSyntax throwStatement)
        {
            // Get the method and class containing the throw statement
            var methodDeclaration = GetContainingMethod(throwStatement);
            var classDeclaration = GetContainingClass(throwStatement);

            if (methodDeclaration.IsNull() || classDeclaration.IsNull())
            {
                yield break;
            }

            // Get the full class name
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol.IsNull())
            {
                yield break;
            }

            var fullyQualifiedClassName = classSymbol.ToDisplayString();

            // Get the method name
            var methodName = methodDeclaration.Identifier.Text;

            // Get the exception type
            var objectCreation = throwStatement.Expression.As<ObjectCreationExpressionSyntax>();

            if (objectCreation.IsNull())
            {
                yield break;
            }

            var constructorSymbol = semanticModel.GetSymbolInfo(objectCreation).Symbol.As<IMethodSymbol>();

            if (constructorSymbol.IsNull())
            {
                yield break;
            }

            if (objectCreation.ArgumentList.IsNull())
            {
                yield break;
            }

            // Generate the text key for each parameter
            for (var i = 0; i < objectCreation.ArgumentList.Arguments.Count; i++)
            {
                var argument = objectCreation.ArgumentList.Arguments[i];

                if (constructorSymbol.Parameters.Length.IsLessOrEqual(i))
                {
                    break;
                }

                var parameter = constructorSymbol.Parameters[i];

                // Check if the argument is a string literal expression
                if (argument.Expression is LiteralExpressionSyntax literalExpression &&
                    literalExpression.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    // Get the parameter name (e.g., "message" for ArgumentException)
                    var parameterName = parameter.Name.FirstCharToUpper();

                    // Get the string value from the literal expression
                    var stringValue = literalExpression.Token.ValueText;

                    // Combine the fully qualified class name, method name, exception type, and parameter name, and text value itself
                    // Sample:
                    // throw new ArgumentException("This is the exception message");
                    // MyAssembly.MyNamespace.MyClass.MyMethod.ArgumentException.Message.This is the exception message
                    var parameterKey = $"{fullyQualifiedClassName}.{methodName}.{constructorSymbol.ContainingType.Name}.{parameterName}.{stringValue}";

                    // Return the key and the string value as a tuple
                    yield return new TextToTranslate(parameterKey, stringValue);
                }
            }
        }
    }

    internal static class AddExtractStringsToLocalizeExtension
    {
        internal static void AddExtractStringsToLocalize(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ExtractStringsToLocalize>();
        }
    }

    internal sealed class ExtractStringsToLocalize
    {
        // Hint: This is a prototype to check if we are able to do a full automation of the localization process.
        //
        //       This fixture allows you to localize all exception messages in all available language files.
        //       This code parse all the exception message strings out and create the keys and its default language "english" -> "en"
        //       into the language files.
        public async Task<ImmutableList<StringsToLocalize>> ExtractLocalizableStrings(ImmutableList<string> languages,
                                                                                      string solutionPath)
        {
            var builder = ImmutableList.CreateBuilder<StringsToLocalize>();

            var workspace = MSBuildWorkspace.Create();

            // Load the entire solution
            var solution = await workspace.OpenSolutionAsync(solutionPath);

            foreach (var project in solution.Projects)
            {
                if (project.IsNull())
                {
                    continue;
                }

                if (project.FilePath.IsNullOrWhiteSpace())
                {
                    continue;
                }

                if (project.Name.Contains(".Test", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var compilation = await project.GetCompilationAsync();

                if (compilation.IsNull())
                {
                    continue;
                }

                var existingTextToTranslate = new Dictionary<string, string>();

                // Iterate over each document in the project
                foreach (var document in project.Documents)
                {
                    var syntaxTree = await document.GetSyntaxTreeAsync();

                    if (syntaxTree.IsNull())
                    {
                        continue;
                    }

                    var semanticModel = compilation.GetSemanticModel(syntaxTree);

                    var throwStatements = FindAllThrowStatements(syntaxTree);

                    foreach (var throwStatement in throwStatements)
                    {
                        var translatableStrings = GetTranslatableStrings(semanticModel, throwStatement).ToList();

                        foreach (var translatableString in translatableStrings)
                        {
                            existingTextToTranslate[translatableString.Key] = translatableString.Text ?? string.Empty;
                        }
                    }
                }

                builder.Add(new StringsToLocalize(project, existingTextToTranslate));
            }

            return builder.ToImmutable();
        }

        private static IEnumerable<ThrowStatementSyntax> FindAllThrowStatements(SyntaxTree syntaxTree)
        {
            var root = syntaxTree.GetRoot();

            return root.DescendantNodes().OfType<ThrowStatementSyntax>()
                       .Where(ts => ts.Expression!.Is<ObjectCreationExpressionSyntax>());
        }

        private static MethodDeclarationSyntax? GetContainingMethod(SyntaxNode? node)
        {
            while (node != null)
            {
                if (node is MethodDeclarationSyntax methodDeclaration)
                {
                    return methodDeclaration;
                }

                node = node.Parent;
            }

            return null;
        }

        private static ClassDeclarationSyntax? GetContainingClass(SyntaxNode? node)
        {
            while (node != null)
            {
                if (node is ClassDeclarationSyntax classDeclaration)
                {
                    return classDeclaration;
                }

                node = node.Parent;
            }

            return null;
        }

        private static IEnumerable<TextToTranslate> GetTranslatableStrings(SemanticModel semanticModel,
                                                                           ThrowStatementSyntax throwStatement)
        {
            // Get the method and class containing the throw statement
            var methodDeclaration = GetContainingMethod(throwStatement);
            var classDeclaration = GetContainingClass(throwStatement);

            if (methodDeclaration.IsNull() || classDeclaration.IsNull())
            {
                yield break;
            }

            // Get the full class name
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol.IsNull())
            {
                yield break;
            }

            var fullyQualifiedClassName = classSymbol.ToDisplayString();

            // Get the method name
            var methodName = methodDeclaration.Identifier.Text;

            // Get the exception type
            var objectCreation = throwStatement.Expression.As<ObjectCreationExpressionSyntax>();

            if (objectCreation.IsNull())
            {
                yield break;
            }

            var constructorSymbol = semanticModel.GetSymbolInfo(objectCreation).Symbol.As<IMethodSymbol>();

            if (constructorSymbol.IsNull())
            {
                yield break;
            }

            if (objectCreation.ArgumentList.IsNull())
            {
                yield break;
            }

            // Generate the text key for each parameter
            for (var i = 0; i < objectCreation.ArgumentList.Arguments.Count; i++)
            {
                var argument = objectCreation.ArgumentList.Arguments[i];

                if (constructorSymbol.Parameters.Length.IsLessOrEqual(i))
                {
                    break;
                }

                var parameter = constructorSymbol.Parameters[i];

                // Check if the argument is a string literal expression
                if (argument.Expression is LiteralExpressionSyntax literalExpression &&
                    literalExpression.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    // Get the parameter name (e.g., "message" for ArgumentException)
                    var parameterName = parameter.Name.FirstCharToUpper();

                    // Get the string value from the literal expression
                    var stringValue = literalExpression.Token.ValueText;

                    // Combine the fully qualified class name, method name, exception type, and parameter name, and text value itself
                    // Sample:
                    // throw new ArgumentException("This is the exception message");
                    // MyAssembly.MyNamespace.MyClass.MyMethod.ArgumentException.Message.This is the exception message
                    var parameterKey = $"{fullyQualifiedClassName}.{methodName}.{constructorSymbol.ContainingType.Name}.{parameterName}.{stringValue}";

                    // Return the key and the string value as a tuple
                    yield return new TextToTranslate(parameterKey, stringValue);
                }
            }
        }
    }

    internal sealed record TextToTranslate(string Key,
                                           string Text);

    internal sealed record StringsToLocalize(Project Project,
                                             Dictionary<string, string> TextToLocalize);
}
