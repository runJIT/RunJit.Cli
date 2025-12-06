using System.Text.RegularExpressions;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Services
{
    internal static class AddSolutionFileServiceExtension
    {
        internal static void AddSolutionFileService(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<SolutionFileService>();
        }
    }

    // Ugly workaround because .NET CLI doesn't support solution folders for arbitrary files.
    // This version extends your existing approach to handle recursion and nesting.
    public class SolutionFileService
    {
        // The well-known GUID for "Solution Folder" in Visual Studio solutions
        // (matches what you've used in your existing code).
        private const string SOLUTION_FOLDER_GUID = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";

        /// <summary>
        ///     Adds or updates a single solution folder by name, inserting the given files if not already present.
        ///     Returns the updated lines. (Same as your existing version, slightly refactored.)
        /// </summary>
        public List<string> AddOrUpdateSolutionFolder(List<string> solutionFileAsLines,
                                                      FileInfo solutionFile,
                                                      string folderName,
                                                      params FileInfo[] files)
        {
            var folderStartIndex = FindSolutionFolderStartIndex(solutionFileAsLines, folderName);

            if (folderStartIndex.EqualsTo(-1))
            {
                // Folder does not exist -> create it
                InsertNewSolutionFolderBlock(solutionFileAsLines, folderName, solutionFile,
                                             files);
            }
            else
            {
                // Folder exists -> add files
                InsertFilesIntoExistingFolder(solutionFileAsLines, folderStartIndex, solutionFile,
                                              files);
            }

            return solutionFileAsLines;
        }

        /// <summary>
        ///     Recursively add the folder (and any subfolders/files) to the .sln,
        ///     mirroring the structure as nested solution folders.
        /// </summary>
        /// <param name="solutionLines"></param>
        /// <param name="startFolderFrom">
        ///     The full path to the folder you want to import (e.g. the ".github" folder).
        /// </param>
        /// <param name="parentFolderName">
        ///     The name of the parent solution folder in which this folder should nest.
        ///     If null or empty, it goes at the solution root.
        /// </param>
        /// <param name="solutionFile"></param>
        public List<string> AddOrUpdateSolutionFolderRecursively(FileInfo solutionFile,
                                                                 List<string> solutionLines,
                                                                 DirectoryInfo startFolderFrom,
                                                                 string? parentFolderName = null)
        {
            // 1) Build an in-memory tree of all subfolders & files
            var rootNode = BuildFolderTree(solutionFile, startFolderFrom, parentFolderName);

            // 2) Insert that tree into the solution
            //    We'll keep a dictionary childGuid -> parentGuid for final "NestedProjects" updates.
            var nestedProjects = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InsertFolderRecursivelyIntoSolution(solutionFile, solutionLines, rootNode,
                                                nestedProjects);

            // 3) Write or update GlobalSection(NestedProjects) with child=parent mappings
            if (nestedProjects.Count > 0)
            {
                UpdateNestedProjectsSection(solutionLines, nestedProjects);
            }

            return solutionLines;
        }

        /// <summary>
        ///     Locates a solution folder block by folder name, e.g.:
        ///     Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "FOLDERNAME", "FOLDERNAME", "{GUID}"
        ///     Returns the line index where that block begins, or -1 if not found.
        /// </summary>
        private static int FindSolutionFolderStartIndex(List<string> solutionLines,
                                                        string folderName)
        {
            // Regex pattern to find:
            // Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "FOLDER", "FOLDER", "{...GUID...}"
            var pattern = $@"Project\(""{Regex.Escape(SOLUTION_FOLDER_GUID)}""\)\s*=\s*""{Regex.Escape(folderName)}"",\s*""{Regex.Escape(folderName)}"",\s*""{{[A-Z0-9-]+}}""";

            for (var i = 0; i < solutionLines.Count; i++)
            {
                if (Regex.IsMatch(solutionLines[i], pattern, RegexOptions.IgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Inserts a new folder block before "Global", including a ProjectSection(SolutionItems).
        /// </summary>
        private static void InsertNewSolutionFolderBlock(List<string> solutionLines,
                                                         string folderName,
                                                         FileInfo solutionFile,
                                                         FileInfo[] files)
        {
            var globalIndex = solutionLines.FindIndex(l => l.TrimStart().StartsWith("Global", StringComparison.Ordinal));

            if (globalIndex < 0)
            {
                throw new InvalidOperationException("Could not find 'Global' section in .sln.");
            }

            var folderGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();

            // e.g. "{12345678-ABCD-1234-ABCD-1234567890AB}"

            var block = new List<string>
                        {
                            $"Project(\"{SOLUTION_FOLDER_GUID}\") = \"{folderName}\", \"{folderName}\", \"{folderGuid}\"",
                            "    ProjectSection(SolutionItems) = preProject"
                        };

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(solutionFile.Directory!.FullName, file.FullName);
                block.Add($"        {relativePath} = {relativePath}");
            }

            block.Add("    EndProjectSection");
            block.Add("EndProject");

            solutionLines.InsertRange(globalIndex, block);
        }

        /// <summary>
        ///     Finds the existing folder's ProjectSection(SolutionItems) and inserts new files,
        ///     or creates one if missing. Avoids duplicating lines.
        /// </summary>
        private static void InsertFilesIntoExistingFolder(List<string> solutionLines,
                                                          int folderStartIndex,
                                                          FileInfo solutionFile,
                                                          FileInfo[] files)
        {
            // find "EndProject" for this folder block
            var projectEndIndex = -1;

            for (var i = folderStartIndex + 1; i < solutionLines.Count; i++)
            {
                if (solutionLines[i].TrimStart().StartWith("EndProject"))
                {
                    projectEndIndex = i;

                    break;
                }
            }

            if (projectEndIndex.EqualsTo(-1))
            {
                throw new InvalidOperationException("Could not find 'EndProject' for existing solution folder.");
            }

            // find or create ProjectSection(SolutionItems)
            var sectionStartIndex = -1;
            var sectionEndIndex = -1;

            for (var i = folderStartIndex + 1; i < projectEndIndex; i++)
            {
                if (solutionLines[i].TrimStart().StartWith("ProjectSection(SolutionItems)"))
                {
                    sectionStartIndex = i;

                    // locate EndProjectSection
                    for (var j = i + 1; j.IsLessOrEqual(projectEndIndex); j++)
                    {
                        if (solutionLines[j].TrimStart().StartWith("EndProjectSection"))
                        {
                            sectionEndIndex = j;

                            break;
                        }
                    }

                    break;
                }
            }

            if (sectionStartIndex.EqualsTo(-1))
            {
                // No ProjectSection => create one just before EndProject
                sectionStartIndex = projectEndIndex;
                var newSection = new List<string> { "    ProjectSection(SolutionItems) = preProject" };

                foreach (var f in files)
                {
                    var relativePath = Path.GetRelativePath(solutionFile.Directory!.FullName, f.FullName);
                    newSection.Add($"        {relativePath} = {relativePath}");
                }

                newSection.Add("    EndProjectSection");

                solutionLines.InsertRange(sectionStartIndex, newSection);
            }
            else
            {
                // Insert new files (avoid duplicates)
                var existingFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for (var i = sectionStartIndex + 1; i < sectionEndIndex; i++)
                {
                    var line = solutionLines[i].Trim();

                    if (!line.IsNullOrEmpty() && !line.StartWith("EndProjectSection"))
                    {
                        var parts = line.Split('=');

                        if (parts.Length.EqualsTo(2))
                        {
                            existingFiles.Add(parts[0].Trim());
                        }
                    }
                }

                var newLines = new List<string>();

                foreach (var f in files)
                {
                    var relativePath = Path.GetRelativePath(solutionFile.Directory!.FullName, f.FullName);

                    if (!existingFiles.Contains(relativePath))
                    {
                        newLines.Add($"        {relativePath} = {relativePath}");
                    }
                }

                if (newLines.Count > 0)
                {
                    solutionLines.InsertRange(sectionEndIndex, newLines);
                }
            }
        }

        /// <summary>
        ///     Builds a simple folder tree structure in memory.
        ///     Each node knows its "Name" (for the solution folder), "ParentFolderName",
        ///     subfolders, and files.
        ///     Using "Name" for the solution folder node is simple but might cause collisions if
        ///     multiple subfolders share the same name in different paths.
        ///     For a robust approach, you might store a full path or unique ID.
        ///     For this example, we'll just store the subfolder name.
        /// </summary>
        private FolderNode BuildFolderTree(FileInfo solutionFile,
                                           DirectoryInfo folderPath,
                                           string? parentFolderName)
        {
            var info = folderPath;

            if (!info.Exists)
            {
                throw new DirectoryNotFoundException($"Folder not found: {folderPath}");
            }

            var node = new FolderNode
                       {
                           Name = info.Name, // e.g. ".github", "ISSUE_TEMPLATE"
                           ParentName = parentFolderName,
                           FolderFullPath = info.FullName
                       };

            // Gather all direct files
            // (Exclude hidden/system if desired)
            var files = info.GetFiles("*", SearchOption.TopDirectoryOnly)
                            .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
                            .ToArray();

            node.Files.AddRange(files);

            // Recurse subdirectories
            foreach (var dir in info.GetDirectories("*", SearchOption.TopDirectoryOnly)
                                    .Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden)))
            {
                var childNode = BuildFolderTree(solutionFile, dir, node.Name);
                node.Subfolders.Add(childNode);
            }

            return node;
        }

        /// <summary>
        ///     Recursively inserts a FolderNode into the .sln, calling AddOrUpdateSolutionFolder for each level.
        ///     Also collects (childGuid, parentGuid) pairs to fix nesting in GlobalSection(NestedProjects).
        /// </summary>
        private List<string> InsertFolderRecursivelyIntoSolution(FileInfo solutionFile,
                                                                 List<string> solutionLines,
                                                                 FolderNode node,
                                                                 Dictionary<string, string> nestedProjectsMap)
        {
            // 1) Create or update a solution folder for "node.Name", placed at root or under "node.ParentName"
            //    Convert the node's files to FileInfo[] so we can pass them to AddOrUpdateSolutionFolder
            var files = node.Files.ToArray();

            AddOrUpdateSolutionFolder(solutionLines, solutionFile, node.Name,
                                      files);

            // 2) Retrieve the GUID of this newly created (or existing) folder
            var childGuid = FindFolderGuidByName(solutionLines, node.Name);

            if (childGuid.IsNullOrEmpty())
            {
                throw new InvalidOperationException($"Could not find GUID for solution folder '{node.Name}' after creation.");
            }

            // 3) If it has a parent, retrieve the parent's GUID and store child=parent in the dictionary
            if (!node.ParentName.IsNullOrEmpty())
            {
                // The parent must already exist in the .sln
                var parentGuid = FindFolderGuidByName(solutionLines, node.ParentName);

                if (!parentGuid.IsNullOrEmpty())
                {
                    // Record the child-parent relationship for NestedProjects
                    nestedProjectsMap[childGuid] = parentGuid;
                }

                // If the parent doesn't exist, that means the parent's creation is not done yet or
                // there's a naming conflict.
                // In a more robust approach, you might handle that differently or
                // reorder your folder creation so parents are always created first.
            }

            // 4) Recurse for subfolders
            foreach (var subfolder in node.Subfolders)
            {
                InsertFolderRecursivelyIntoSolution(solutionFile, solutionLines, subfolder,
                                                    nestedProjectsMap);
            }

            return solutionLines;
        }

        /// <summary>
        ///     Searches for a solution folder block with the given name,
        ///     and extracts the {GUID} from this line:
        ///     Project("{2150E333-...}") = "NAME", "NAME", "{THE-GUID}"
        ///     Returns the GUID string including braces, e.g. "{ABC...}"
        ///     or null if not found.
        /// </summary>
        private static string? FindFolderGuidByName(List<string> solutionLines,
                                                    string folderName)
        {
            // We look for:
            // Project("{2150E333-...}") = "folderName", "folderName", "{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}"
            var pattern = $@"Project\(""{Regex.Escape(SOLUTION_FOLDER_GUID)}""\)\s*=\s*""{Regex.Escape(folderName)}"",\s*""{Regex.Escape(folderName)}"",\s*""(?<guid>{{[A-Z0-9-]+}})""";

            for (var i = 0; i < solutionLines.Count; i++)
            {
                var match = Regex.Match(solutionLines[i], pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    return match.Groups["guid"].Value;

                    // e.g. "{9543FC93-BCD3-4322-BB85-8A0F731383B8}"
                }
            }

            return null;
        }

        private void UpdateNestedProjectsSection(
            List<string> lines,
            Dictionary<string, string> nestedProjectsMap)
        {
            if (nestedProjectsMap.Count.EqualsTo(0))
            {
                return;
            }

            // 1) Locate top-level Global ... EndGlobal
            int globalIndex = lines.FindIndex(l => l.TrimStart().Equals("Global", StringComparison.OrdinalIgnoreCase));

            if (globalIndex < 0)
            {
                throw new InvalidOperationException("Malformed .sln: missing top-level 'Global'.");
            }

            int endGlobalIndex = lines.FindIndex(globalIndex + 1, l =>
                                                                      l.TrimStart().Equals("EndGlobal", StringComparison.OrdinalIgnoreCase));

            if (endGlobalIndex < 0)
            {
                throw new InvalidOperationException("Malformed .sln: missing top-level 'EndGlobal'.");
            }

            // 2) We now search for an existing top-level "GlobalSection(NestedProjects)" block
            //    within the lines between globalIndex+1 and endGlobalIndex-1.
            //    We'll skip entire sections if they are not NestedProjects.
            int nestedStartIndex = -1;
            int nestedEndIndex = -1;
            int i = globalIndex + 1;

            while (i < endGlobalIndex)
            {
                var line = lines[i].TrimStart();

                if (line.StartWith("GlobalSection("))
                {
                    // e.g. "GlobalSection(SolutionConfigurationPlatforms) = preSolution"
                    // or    "GlobalSection(NestedProjects) = preSolution"

                    // Check if it's NestedProjects:
                    if (line.StartWith("GlobalSection(NestedProjects)"))
                    {
                        nestedStartIndex = i;

                        // find matching EndGlobalSection
                        for (int j = i + 1; j < endGlobalIndex; j++)
                        {
                            if (lines[j].TrimStart().StartWith("EndGlobalSection"))
                            {
                                nestedEndIndex = j;

                                break;
                            }
                        }

                        // IMPORTANT: break the while-loop if we found the NestedProjects section
                        break;
                    }
                    else
                    {
                        // Some other GlobalSection(...) => skip until its matching EndGlobalSection
                        int sectionClose = -1;

                        for (int j = i + 1; j < endGlobalIndex; j++)
                        {
                            if (lines[j].TrimStart().StartWith("EndGlobalSection"))
                            {
                                sectionClose = j;

                                break;
                            }
                        }

                        if (sectionClose.EqualsTo(-1))
                        {
                            throw new InvalidOperationException("Malformed .sln: 'GlobalSection(...)' without matching 'EndGlobalSection'.");
                        }

                        // jump i just past this section
                        i = sectionClose + 1;

                        continue;
                    }
                }
                else
                {
                    // not a GlobalSection(...) => just move on
                    i++;
                }
            }

            if (nestedStartIndex.EqualsTo(-1))
            {
                // 3) No NestedProjects section found -> create one at top-level, just before EndGlobal
                var newSection = new List<string>();
                newSection.Add("\tGlobalSection(NestedProjects) = preSolution");

                // child -> parent lines
                foreach (var kvp in nestedProjectsMap)
                {
                    newSection.Add($"\t\t{kvp.Key} = {kvp.Value}");
                }

                newSection.Add("\tEndGlobalSection");

                lines.InsertRange(endGlobalIndex, newSection);
            }
            else
            {
                // 4) We have an existing NestedProjects section from nestedStartIndex..nestedEndIndex
                // Add new lines before the EndGlobalSection
                var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for (int lineIndex = nestedStartIndex + 1; lineIndex < nestedEndIndex; lineIndex++)
                {
                    var row = lines[lineIndex].Trim();

                    // e.g. "{childGuid} = {parentGuid}"
                    if (!row.StartWith("EndGlobalSection"))
                    {
                        var parts = row.Split('=');

                        if (parts.Length.EqualsTo(2))
                        {
                            existing.Add(parts[0].Trim());
                        }
                    }
                }

                // Insert any missing lines
                var newLines = new List<string>();

                foreach (var kvp in nestedProjectsMap)
                {
                    if (!existing.Contains(kvp.Key))
                    {
                        newLines.Add($"\t\t{kvp.Key} = {kvp.Value}");
                    }
                }

                if (newLines.Count > 0)
                {
                    lines.InsertRange(nestedEndIndex, newLines);
                }
            }
        }
    }

    /// <summary>
    ///     A simple in-memory representation of a folder node: Name, subfolders, files.
    ///     "ParentName" is used to locate the parent solution folder in the .sln.
    /// </summary>
    internal sealed class FolderNode
    {
        public string Name { get; set; } = null!;

        public string? ParentName { get; set; } // null if root-level

        public string FolderFullPath { get; set; } = null!;

        public List<FileInfo> Files { get; } = new();

        public List<FolderNode> Subfolders { get; } = new();
    }
}
