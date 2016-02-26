using System.IO;
using SexyReact;

namespace SexyDb.Tests
{
    [Rx]
    public class TestDatabase : SexyDatabase
    {
        public TestDatabase() : base(CreateFolder())
        {
        }

        public TestDatabase(string folder) : base(folder)
        {
        }

        public static string CreateFolder()
        {
            var fileName = Path.GetTempFileName();
            File.Delete(fileName);
            return fileName;
        }
    }
}