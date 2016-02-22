using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SexyDb.Tests
{
    public static class FileExtensions
    {
        public static async Task Edit(this FileInfo file, string value)
        {
            var lastModified = file.LastWriteTime;
            File.WriteAllText(file.FullName, value);
            while (lastModified == file.LastWriteTime)
            {
                await Task.Delay(1);
                file.Refresh();
            }
        }
    }
}