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
            File.WriteAllText(file.FullName, value);
            await Task.Delay(100);
            file.Refresh();
        }
    }
}