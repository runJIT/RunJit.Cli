using System.Xml.Linq;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.New.NugetProject
{
    public static class AddNugetProjectSettingsExtension
    {
        public static void AddNugetPackageProjectSettings(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<INugetProjectContractsSpecificCodeGen, NugetPackageProjectSettings>();
            services.AddSingletonIfNotExists<INugetProjectSpecificCodeGen, NugetPackageProjectSettings>();
        }
    }

    internal class NugetPackageProjectSettings : INugetProjectSpecificCodeGen,
                                                 INugetProjectContractsSpecificCodeGen
    {
        public Task GenerateAsync(FileInfo projectFileInfo,
                                  FileInfo solutionFile,
                                  XDocument projectDocument,
                                  NugetProjectInfos minimalApiProjectInfos)
        {
            // The parts we need:
            //
            //<?xml version="1.0" encoding="utf-8"?>
            //<Project Sdk="Microsoft.NET.Sdk">
            //    <PropertyGroup>
            //        <!--Nuget specific package infos-->
            //        <TargetFramework>net9.0</TargetFramework>
            //        <ImplicitUsings>enable</ImplicitUsings>
            //        <Nullable>enable</Nullable>
            //        <Authors>Philip Pregler</Authors>
            //        <Company>Siemens AG</Company>
            //        <Description>
            //            A library which contains following functions:
            //            - Siemens.AspNet.ErrorHandler
            //        </Description>
            //        <PackageProjectUrl>https://www.nuget.org/packages/Siemens.AspNet.ErrorHandler</PackageProjectUrl>
            //        <RepositoryUrl>https://www.nuget.org/packages/Siemens.AspNet.ErrorHandler</RepositoryUrl>
            //        <PackageTags>Siemens.AspNet.ErrorHandler;</PackageTags>
            //        <Copyright>Copyright 2025 (c) Siemens AG. All rights reserved.</Copyright>
            //        <PackageLicenseFile>LICENSE.md</PackageLicenseFile>
            //        <AllowedOutputExtensionsInPackageBuildOutputFolder>$(AllowedOutputExtensionsInPackageBuildOutputFolder);.pdb</AllowedOutputExtensionsInPackageBuildOutputFolder>
            //        <PackageReadmeFile>README.md</PackageReadmeFile>
            //        <PackageIcon>nuget-package-icon.png</PackageIcon>
            //        <IsPackable>true</IsPackable>
            //        <IsPublishable>false</IsPublishable>
            //        <Title>Siemens.AspNet.ErrorHandler</Title>
            //    </PropertyGroup>
            //    <ItemGroup>
            //        <PackageReference Include="Extensions.Pack" Version="6.0.6" />
            //        <PackageReference Include="GitVersion.MsBuild" Version="5.12.0">
            //            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
            //            <PrivateAssets>all</PrivateAssets>
            //        </PackageReference>
            //        <PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.3" />
            //        <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.3" />
            //    </ItemGroup>
            //    <!--Embedded files area-->
            //    <ItemGroup>
            //        <EmbeddedResource Include="**\*.txt" Exclude="bin\**\*;obj\**\*" />
            //        <EmbeddedResource Include="**\*.json" Exclude="bin\**\*;obj\**\*" />
            //        <EmbeddedResource Include="**\*.sh" Exclude="bin\**\*;obj\**\*" />
            //    </ItemGroup>
            //    <ItemGroup>
            //        <None Include="..\..\LICENSE.md">
            //            <Pack>True</Pack>
            //            <PackagePath>\</PackagePath>
            //        </None>
            //        <None Include="..\..\nuget-package-icon.png">
            //            <Pack>True</Pack>
            //            <PackagePath>\</PackagePath>
            //        </None>
            //    </ItemGroup>
            //    <ItemGroup>
            //        <None Update="README.md">
            //            <Pack>True</Pack>
            //            <PackagePath>\</PackagePath>
            //        </None>
            //    </ItemGroup>
            //</Project>

            // Load the csproj XML document
            var projectElement = projectDocument.Root!;
            var packageName = projectFileInfo.NameWithoutExtension();

            // Get or create a PropertyGroup element
            var propertyGroup = projectElement.Elements("PropertyGroup").FirstOrDefault();

            if (propertyGroup == null)
            {
                propertyGroup = new XElement("PropertyGroup");
                projectElement.Add(propertyGroup);
            }

            // Add an XML comment to indicate NuGet specific package infos if not already added
            if (!propertyGroup.Nodes().OfType<XComment>().Any(c => c.Value.Contains("Nuget specific package infos")))
            {
                propertyGroup.AddFirst(new XComment("Nuget specific package infos"));
            }

            // Update or add required properties
            SetOrUpdateElement(propertyGroup, "Authors", "Philip Pregler");
            SetOrUpdateElement(propertyGroup, "Company", "Siemens AG");

            // Build the description dynamically
            var description = $"A library which contains following functions:\n- {packageName}";
            SetOrUpdateElement(propertyGroup, "Description", description);
            SetOrUpdateElement(propertyGroup, "PackageProjectUrl", $"https://www.nuget.org/packages/{packageName}");
            SetOrUpdateElement(propertyGroup, "RepositoryUrl", $"https://www.nuget.org/packages/{packageName}");
            SetOrUpdateElement(propertyGroup, "PackageTags", $"{packageName};");
            SetOrUpdateElement(propertyGroup, "Copyright", $"Copyright {DateTime.UtcNow.Year} (c) Siemens AG. All rights reserved.");
            SetOrUpdateElement(propertyGroup, "PackageLicenseFile", "LICENSE.md");
            SetOrUpdateElement(propertyGroup, "AllowedOutputExtensionsInPackageBuildOutputFolder", "$(AllowedOutputExtensionsInPackageBuildOutputFolder);.pdb");
            SetOrUpdateElement(propertyGroup, "PackageReadmeFile", "README.md");
            SetOrUpdateElement(propertyGroup, "PackageIcon", "nuget-package-icon.png");
            SetOrUpdateElement(propertyGroup, "IsPackable", "true");
            SetOrUpdateElement(propertyGroup, "IsPublishable", "false");
            SetOrUpdateElement(propertyGroup, "Title", packageName);

            // Update or create project specific readme
            // <None Include="..\..\..\Siemens.AspNet.ErrorHandler\src\Siemens.AspNet.ErrorHandler\README.md">
            UpdateOrCreateItemGroup(projectElement,
                                    itemInclude: $@"..\..\..\{minimalApiProjectInfos.NormalizedName}\src\{minimalApiProjectInfos.NormalizedName}\README.md",
                                    packValue: "True",
                                    packagePath: @"\");

            // Update or create ItemGroup for the nuget-package-icon.png
            UpdateOrCreateItemGroup(projectElement,
                                    itemInclude: @"..\..\nuget-package-icon.png",
                                    packValue: "True",
                                    packagePath: @"\");

            // Update or create ItemGroup for the LICENSE.md file
            UpdateOrCreateItemGroup(projectElement,
                                    itemInclude: @"..\..\LICENSE.md",
                                    packValue: "True",
                                    packagePath: @"\");

            return Task.CompletedTask;
        }

        // Helper method to update or create a child element with a given value
        static void SetOrUpdateElement(XElement parent,
                                       string elementName,
                                       string value)
        {
            var element = parent.Element(elementName);

            if (element == null)
            {
                element = new XElement(elementName, value);
                parent.Add(element);
            }
            else
            {
                element.Value = value;
            }
        }

        // Helper method to update or create an ItemGroup for a file reference
        static void UpdateOrCreateItemGroup(XElement projectElement,
                                            string itemInclude,
                                            string packValue,
                                            string packagePath)
        {
            // Find an existing ItemGroup with a <None> element matching the Include attribute
            var itemGroup = projectElement.Elements("ItemGroup")
                                          .FirstOrDefault(ig => ig.Elements("None")
                                                                  .Any(n => n.Attribute("Include")?.Value == itemInclude));

            if (itemGroup == null)
            {
                // If not found, create a new ItemGroup and add the <None> element
                itemGroup = new XElement("ItemGroup");
                var noneElement = new XElement("None", new XAttribute("Include", itemInclude));
                noneElement.Add(new XElement("Pack", packValue));
                noneElement.Add(new XElement("PackagePath", packagePath));
                itemGroup.Add(noneElement);
                projectElement.Add(itemGroup);
            }
            else
            {
                // Update existing <None> element
                var noneElement = itemGroup.Elements("None")
                                           .First(n => n.Attribute("Include")?.Value == itemInclude);

                SetOrUpdateElement(noneElement, "Pack", packValue);
                SetOrUpdateElement(noneElement, "PackagePath", packagePath);
            }
        }
    }
}
