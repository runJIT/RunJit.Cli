using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Services
{
    internal static class AddRenameFilesAndFoldersExtension
    {
        internal static void AddRenameFilesAndFolders(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRenameFilesAndFolders, RenameFilesAndFolders>();
        }
    }

    internal interface IRenameFilesAndFolders
    {
        DirectoryInfo Rename(DirectoryInfo directoryInfo,
                             string originalName,
                             string newName);

        void Rename2(DirectoryInfo directoryInfo,
                     string originalName,
                     string newName);
    }

    internal sealed class RenameFilesAndFolders : IRenameFilesAndFolders
    {
        public DirectoryInfo Rename(DirectoryInfo directoryInfo,
                                    string originalName,
                                    string newName)
        {
            // Check the new target folder exists

            var newRootFolder = new DirectoryInfo(directoryInfo.FullName.Replace(originalName, newName));

            if (newRootFolder.Exists)
            {
                newRootFolder.Delete(true);
            }

            if (directoryInfo.FullName.Contains(originalName))
            {
                try
                {
                    Directory.Move(directoryInfo.FullName, newRootFolder.FullName);
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                    Directory.Move(directoryInfo.FullName, newRootFolder.FullName);
                }
            }

            var folders = newRootFolder.EnumerateDirectories("*.*", SearchOption.AllDirectories).ToList();

            foreach (var folder in folders)
            {
                // Special and hidden folders like .git, .vs those should not be renamed
                if (folder.Name.StartsWith("."))
                {
                    continue;
                }

                if (folder.Name.Contains(originalName))
                {
                    var destDirName = folder.FullName.Replace(originalName, newName);
                    Directory.Move(folder.FullName, destDirName);
                }
            }

            // all root files
            var newFolders = newRootFolder.EnumerateDirectories("*.*", SearchOption.AllDirectories).ToList();
            newFolders.Add(newRootFolder);

            foreach (var folder in newFolders)
            {
                // Special and hidden folders like .git, .vs those should not be renamed
                if (folder.Name.StartsWith("."))
                {
                    continue;
                }

                foreach (var fileInfo in folder.EnumerateFiles())
                {
                    if (fileInfo.Name.StartsWith("."))
                    {
                        continue;
                    }

                    var content = File.ReadAllText(fileInfo.FullName);

                    if (content.Contains(originalName))
                    {
                        var newContent = content.Replace(originalName, newName);
                        File.WriteAllText(fileInfo.FullName, newContent);
                    }

                    if (fileInfo.Name.Contains(originalName))
                    {
                        File.Move(fileInfo.FullName, fileInfo.FullName.Replace(originalName, newName));
                    }
                }
            }

            return newRootFolder;
        }

        public void Rename2(DirectoryInfo directoryInfo,
                            string originalName,
                            string newName)
        {
            var newDirectories = GetNewDirectories();

            IEnumerable<DirectoryInfo> GetNewDirectories()
            {
                var folders = directoryInfo.EnumerateDirectories($"{originalName}*", SearchOption.AllDirectories);

                foreach (var folder in folders)
                {
                    if (folder.Name.Contains(originalName))
                    {
                        var destDirName = folder.FullName.Replace(originalName, newName);

                        if (Directory.Exists(destDirName).IsFalse())
                        {
                            Directory.Move(folder.FullName, destDirName);

                            yield return new DirectoryInfo(destDirName);
                        }
                    }
                }
            }

            var allFiles = newDirectories.SelectMany(f => f.EnumerateFiles("*.*", SearchOption.AllDirectories));

            foreach (var fileInfo in allFiles)
            {
                var content = File.ReadAllText(fileInfo.FullName);

                if (content.Contains(originalName))
                {
                    var newContent = content.Replace(originalName, newName);
                    File.WriteAllText(fileInfo.FullName, newContent);
                }

                if (fileInfo.Name.Contains(originalName))
                {
                    var destFileName = fileInfo.FullName.Replace(originalName, newName);
                    File.Move(fileInfo.FullName, destFileName);
                }
            }
        }
    }
}
