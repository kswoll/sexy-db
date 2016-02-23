using System.IO;

namespace SexyDb.Utils
{
    public static class FileUtils
    {
        public static void DeleteTree(this DirectoryInfo directory)
        {
            string target;
            do
            {
                target = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
            while (File.Exists(target) || Directory.Exists(target));

            Directory.Move(directory.FullName, target);
            DeleteTree(target);
            directory.Refresh();
        }

        private static void DeleteTree(string directory)
        {
            foreach (var child in Directory.GetFiles(directory))
                File.Delete(child);
            foreach (var child in Directory.GetDirectories(directory))
                DeleteTree(child);
            Directory.Delete(directory);
        }
    }
}