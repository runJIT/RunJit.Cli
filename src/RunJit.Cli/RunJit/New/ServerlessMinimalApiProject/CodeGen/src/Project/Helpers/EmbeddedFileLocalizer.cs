using System.Reflection;
using System.Runtime.CompilerServices;
using Extensions.Pack;
using $ProjectName$.Regex;

namespace $ProjectName$.Helpers
{
    internal static class AddEmbeddedFileLocalizerExtension
    {
        internal static void AddEmbeddedFileLocalizer(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<EmbeddedFileLocalizer>();
        }
    }

    /// <summary>
    ///     Localizes the documentation file based on the caller's file path and a specified assembly.
    /// </summary>
    /// <param name="embeddedFile">
    ///     The name of the embedded file to localize.
    /// </param>
    /// <param name="callingAssembly">
    ///     The assembly containing the embedded file.
    /// </param>
    /// <param name="callerFilePath">
    ///     The file path of the caller. This parameter is automatically populated by the compiler.
    /// </param>
    /// <returns>
    ///     The localized file path or the original file name if localization fails.
    /// </returns>
    internal string LocalizeDocumentation(string embeddedFile,
                                          Assembly callingAssembly,
                                          [CallerFilePath] string callerFilePath = "")
    {
        // 1. Extract the file name from the embedded file path
        //    V1.Create.Documentations.Summary.txt
        //    We have to extract the 'Summary.txt' filename only out.
        var parts = embeddedFile.Split('.');
        var lastTwo = parts[^2..]; // take the last two elements
        var embeddedFileName = lastTwo.Flatten(".");

        // 2. Split the full path into segments
        var segments = callerFilePath.Split(Path.DirectorySeparatorChar);

        // 3. Locate the version folder, e.g., one that starts with 'V' followed by a number
        var versionIndex = -1;

        for (var i = 0; i < segments.Length; i++)
        {
            if (GlobalRegex.VersionRegex().IsMatch(segments[i]))
            {
                versionIndex = i;

                break;
            }
        }

        // 4. If no version folder is found or the next folder is missing, return the original file name
        if (versionIndex == -1 || versionIndex + 1 >= segments.Length)
        {
            return embeddedFile;
        }

        // 5. Extract the version folder and the next folder (e.g., "V1" and "Create")
        var versionFolder = segments[versionIndex];
        var nextFolder = segments[versionIndex + 1];

        // 6. Build the new path string with dots
        var newPath = $"{versionFolder}.{nextFolder}.Documentations.{embeddedFileName}";

        // 7. Retrieve the content of the file from the specified assembly
        return callingAssembly.GetFileContentFrom(newPath);
    }
}
