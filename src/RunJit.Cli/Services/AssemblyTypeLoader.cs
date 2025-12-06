using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Configuration;
using RunJit.Cli.RunJit.Generate.Client;

namespace RunJit.Cli.Services
{
    internal static class AddAssemblyTypeLoaderExtension
    {
        internal static void AddAssemblyTypeLoader(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<AssemblyTypeLoader>();
        }
    }

    internal sealed class AssemblyTypeLoader
    {
        internal ImmutableList<Type> GetAllTypesFrom(FileInfo assemblyFile)
        {
            var types = GetAllTypes(assemblyFile);
            var additionalTypes = GetDeepTypeInfos(types).DistinctBy(type => type.FullName);

            var allTypes = types.Concat(additionalTypes).ToImmutableList();

            var allDeclaredTypes = allTypes.DistinctBy(type => type.FullName).ToImmutableList();

            return allDeclaredTypes;
        }

        private ImmutableList<Type> GetAllTypes(FileInfo assemblyFile)
        {
            Directory.SetCurrentDirectory(assemblyFile.Directory!.FullName);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var assembly = Assembly.LoadFrom(assemblyFile.FullName);
            var types = assembly.GetTypes().ToImmutableList();

            return types;
        }

        private IEnumerable<Type> GetDeepTypeInfos(ImmutableList<Type> types)
        {
            foreach (var type in types)
            {
                if (type.DeclaringType.IsNull())
                {
                    continue;
                }

                if (type.Name.StartWith("<"))
                {
                    continue;
                }

                yield return type;

                var genericTypes = type.GetAllTypesFromGenericType().DistinctBy(t => t.FullName);

                foreach (var genericType in genericTypes)
                {
                    yield return genericType;
                }

                var methods = type.GetMethods();

                foreach (var method in methods)
                {
                    var parameters = method.GetParameters();

                    foreach (var parameter in parameters)
                    {
                        yield return parameter.ParameterType;
                    }

                    yield return method.ReturnType;
                }
            }
        }

        private Assembly? CurrentDomain_AssemblyResolve(object? sender,
                                                        ResolveEventArgs args)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var strings = args.Name.Split(',');
            var searchPattern = $"{strings.First()}.dll";

            // 1. Check first if it is already loaded in current app domain
            var alreadyLoaded = assemblies.FirstOrDefault(a => a.GetName().Name.EqualsTo(strings.First()));

            if (alreadyLoaded.IsNotNull())
            {
                return alreadyLoaded;
            }

            // 2. Check current directory
            var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            var dll = currentDirectory.EnumerateFiles(searchPattern).FirstOrDefault();

            if (dll.IsNotNull())
            {
                return Assembly.LoadFrom(dll.FullName);
            }

            // 3. Search in Net 7 runtimes
            var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
            var foundInRuntimeFolder = runtimeAssemblies.FirstOrDefault(file => file.Contains(searchPattern));

            if (foundInRuntimeFolder.IsNotNullOrWhiteSpace())
            {
                Assembly.LoadFrom(foundInRuntimeFolder);
            }

            // 4. If not found search in nuget packages
            //    Very tricky here
            var settings = Settings.LoadDefaultSettings(null);
            var nugetGlobalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(settings);
            var nugetDirectory = new DirectoryInfo(nugetGlobalPackagesFolder);
            var missingPackage = nugetDirectory.EnumerateFiles(searchPattern, SearchOption.AllDirectories).Where(file => file.Directory!.FullName.Contains(strings.First(), StringComparison.OrdinalIgnoreCase)).ToList();
            var containsNet7 = missingPackage.LastOrDefault();
            var fileToLoad = containsNet7?.FullName;

            if (fileToLoad.IsNullOrWhiteSpace())
            {
                return null;
            }
            try
            {
                return Assembly.LoadFrom(fileToLoad);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return null;
            }
        }
    }
}
