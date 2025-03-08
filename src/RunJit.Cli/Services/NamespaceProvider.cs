using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Services
{
    internal static class AddNamespaceProviderExtension
    {
        internal static void AddNamespaceProvider(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<NamespaceProvider>();
        }
    }

    internal sealed class NamespaceProvider
    {
        private const string Template = """
                                        <wpf:ResourceDictionary xml:space="preserve"
                                                                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                                                                xmlns:s="clr-namespace:System;assembly=mscorlib"
                                                                xmlns:ss="urn:shemas-jetbrains-com:settings-storage-xaml"
                                                                xmlns:wpf="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                                        </wpf:ResourceDictionary>
                                        """;

        
        
        //<wpf:ResourceDictionary xml:space="preserve" x
        //    mlns:x="http://schemas.microsoft.com/winfx/2006/xaml" 
        //    xmlns:s="clr-namespace:System;assembly=mscorlib" 
        //    xmlns:ss="urn:shemas-jetbrains-com:settings-storage-xaml" 
        //    xmlns:wpf="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
        //
        //    <s:Boolean x:Key="/Default/CodeInspection/NamespaceProvider/NamespaceFoldersToSkip/=api_005Cprojects_005Cv1_005Ccreate/@EntryIndexedValue">True</s:Boolean>
        //    <s:Boolean x:Key="/Default/CodeInspection/NamespaceProvider/NamespaceFoldersToSkip/=api_005Cprojects_005Cv1_005Cdeleteall/@EntryIndexedValue">True</s:Boolean>
        //</wpf:ResourceDictionary>
        
        
        internal void SetNamespaceProvider(FileInfo projectFile,
                                           string ns,
                                           bool value)
        {
            // Remove the project's default namespace prefix.
            var projectName = Path.GetFileNameWithoutExtension(projectFile.Name);
            var normalizedNamespace = ns.Replace($"{projectName}.", string.Empty);

            // Compute the Resharper ignore entry by joining the lower-cased parts with the escape sequence.
            var resharperIgnoreEntry = normalizedNamespace.Split('.').Select(p => p.ToLowerInvariant()).Flatten("_005C");
            
            // Load or create the DotSettings XML document.
            var (document, filePath) = LoadOrCreateDotSettings(projectFile);

            // Define the XNamespace for the x:Key attribute.
            XNamespace xNs = "http://schemas.microsoft.com/winfx/2006/xaml";

            // Build the expected key value.
            var keyValue = $"/Default/CodeInspection/NamespaceProvider/NamespaceFoldersToSkip/={resharperIgnoreEntry}/@EntryIndexedValue";

            // Check if an element with this key already exists.
            var existingElement = document.Root?
                                          .Elements()
                                          .FirstOrDefault(e => e.Attribute(xNs + "Key")?.Value == keyValue);

            if (existingElement != null)
            {
                // Update the value if it differs.
                if (!string.Equals(existingElement.Value, value.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    existingElement.Value = value.ToString();
                    document.Save(filePath);
                }

                return;
            }

            // Create a new s:Boolean element.
            XNamespace sNs = "clr-namespace:System;assembly=mscorlib";

            var booleanElement = new XElement(sNs + "Boolean",
                                              new XAttribute(xNs + "Key", keyValue),
                                              value.ToString());

            // Add the element to the document and save.
            document.Root?.Add(booleanElement);
            document.Save(filePath);
        }

        private (XDocument document, string filePath) LoadOrCreateDotSettings(FileInfo projectFile)
        {
            // Try to find an existing .DotSettings file in the same directory.
            var dotSettingsFile = projectFile.Directory?
                                             .GetFiles($"{projectFile.Name}.DotSettings")
                                             .FirstOrDefault();

            if (dotSettingsFile != null)
            {
                return (XDocument.Load(dotSettingsFile.FullName), dotSettingsFile.FullName);
            }

            // If not found, create a new XDocument from the template.
            var newDocument = XDocument.Parse(Template);
            var filePath = Path.Combine(projectFile.Directory!.FullName, $"{projectFile.Name}.DotSettings");

            return (newDocument, filePath);
        }
    }
}
