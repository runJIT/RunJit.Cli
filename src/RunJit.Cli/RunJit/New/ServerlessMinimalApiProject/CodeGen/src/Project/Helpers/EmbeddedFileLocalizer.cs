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

    internal sealed class EmbeddedFileLocalizer()
    {
        internal string LocalizeDocumentation(string embeddedFile,
                                              [CallerFilePath] string callerFilePath = "")
        {
            return LocalizeDocumentation(embeddedFile, Assembly.GetCallingAssembly(), callerFilePath);
        }

        internal string LocalizeDocumentation(string embeddedFile,
                                              Assembly callingAssembly,
                                              [CallerFilePath] string callerFilePath = "")
        {
            // Split the full path into segments
            var segments = callerFilePath.Split(Path.DirectorySeparatorChar);
        
            // Locate the version folder, e.g., one that starts with 'V' followed by a number.
            var versionIndex = -1;
            for (var i = 0; i < segments.Length; i++)
            {
                if (GlobalRegex.VersionRegex().IsMatch(segments[i]))
                {
                    versionIndex = i;
                    break;
                }
            }
        
            if (versionIndex == -1 || versionIndex + 1 >= segments.Length)
            {
                return embeddedFile;
            }
        
            // Extract the version folder and the next folder (e.g., "V1" and "Create")
            var versionFolder = segments[versionIndex];
            var nextFolder = segments[versionIndex + 1];
        
            // Build the new path string with dots
            var newPath = $"{versionFolder}.{nextFolder}.Documentations.{embeddedFile}";

            return callingAssembly.GetFileContentFrom(newPath);
        }
    }
}
