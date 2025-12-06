using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.RunJit.Generate.Client;

namespace RunJit.Cli.Services.Resharper
{
    internal static class AddResharperSettingsBuilderExtension
    {
        internal static void AddResharperSettingsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ResharperSettingsBuilder>();
        }
    }

    // What we create here:
    // - We create here the resharper project settings to setup namespace providers correctly
    // 
    // Sample:
    // 
    // <wpf:ResourceDictionary xml:space="preserve" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:s="clr-namespace:System;assembly=mscorlib" xmlns:ss="urn:shemas-jetbrains-com:settings-storage-xaml" xmlns:wpf="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    //     <s:Boolean x:Key="/Default/CodeInspection/NamespaceProvider/NamespaceFoldersToSkip/=api_005Cadmin_005Cv1_005Cmodels/@EntryIndexedValue">True</s:Boolean>
    //     <s:Boolean x:Key="/Default/CodeInspection/NamespaceProvider/NamespaceFoldersToSkip/=api_005Cadmin_005Cv1_005Crequests/@EntryIndexedValue">True</s:Boolean>
    //     <s:Boolean x:Key="/Default/CodeInspection/NamespaceProvider/NamespaceFoldersToSkip/=api_005Cadmin_005Cv1_005Cresponses/@EntryIndexedValue">True</s:Boolean>
    // </wpf:ResourceDictionary>
    internal sealed class ResharperSettingsBuilder
    {
        private readonly string _resharperSettingsEntry = "<s:Boolean x:Key=\"/Default/CodeInspection/NamespaceProvider/NamespaceFoldersToSkip/=$namespace$/@EntryIndexedValue\">True</s:Boolean>";

        private readonly string _resharperSettingsTemplate = """
                                                             <wpf:ResourceDictionary xml:space="preserve" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:s="clr-namespace:System;assembly=mscorlib" xmlns:ss="urn:shemas-jetbrains-com:settings-storage-xaml" xmlns:wpf="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                                                             $namespaces$
                                                             </wpf:ResourceDictionary>
                                                             """;

        public string BuildFrom(GeneratedClient generatedClient)
        {
            var namespaceProviders = NamespaceProviderTrue(generatedClient);
            var entries = namespaceProviders.Select(namesp => _resharperSettingsEntry.Replace("$namespace$", namesp)).Flatten(Environment.NewLine);

            var projectSettings = _resharperSettingsTemplate.Replace("$namespaces$", entries);

            return projectSettings;
        }

        private IEnumerable<string> NamespaceProviderTrue(GeneratedClient dataType)
        {
            foreach (var facade in dataType.Facades)
            {
                foreach (var endpoint in facade.Endpoints)
                {
                    var domain = endpoint.Domain.ToLowerInvariant();
                    var version = endpoint.ControllerInfo.Version?.Normalized.ToLowerInvariant();

                    if (version.IsNotNull())
                    {
                        yield return $"api_005C{domain}_005Cmodels";
                        yield return $"api_005C{domain}_005Crequests";
                        yield return $"api_005C{domain}_005Cresponses";
                    }
                    else
                    {
                        yield return $"api_005C{domain}_005C{version}_005Cmodels";
                        yield return $"api_005C{domain}_005C{version}_005Crequests";
                        yield return $"api_005C{domain}_005C{version}_005Cresponses";
                    }
                }
            }

            yield return "httpcallhandlers";
            yield return "responsetypehandling";
            yield return "extensions";
        }
    }
}
