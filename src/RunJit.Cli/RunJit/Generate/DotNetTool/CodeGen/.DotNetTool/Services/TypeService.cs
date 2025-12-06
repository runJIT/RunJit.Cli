using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddTypeServiceExtension
    {
        internal static void AddTypeService(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<TypeService>();
        }
    }

    internal sealed class TypeService
    {
        public string GetFullQualifiedName(string projectName,
                                           FileInfo fileInfo)
        {
            var path = CollectPath(fileInfo.Directory, projectName).Reverse().ToList();
            var fullQualifiedName = $"{projectName}.{path.Flatten(".")}.{fileInfo.NameWithoutExtension()}";

            return fullQualifiedName;
        }

        private static IEnumerable<string> CollectPath(DirectoryInfo? startDirectoryInfo,
                                                       string name)
        {
            if (startDirectoryInfo.IsNull())
            {
                yield break;
            }

            if (startDirectoryInfo.Name.NotEqualsTo(name))
            {
                // Hint: Structure in the solutions all was normalized, that first char is to upper.
                yield return startDirectoryInfo.Name.FirstCharToUpper();
            }
            else
            {
                yield break;
            }

            var result = CollectPath(startDirectoryInfo.Parent, name).ToList();

            foreach (var value in result)
            {
                // Hint: Structure in the solutions all was normalized, that first char is to upper.
                yield return value.FirstCharToUpper();
            }
        }
    }
}
