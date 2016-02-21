using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SexyDb.Tests
{
    [TestFixture]
    public class SimpleTests 
    {
        [Test]
        public async Task SaveStringProperty()
        {
            var db = new StringPropertyDatabase();
            db.StringProperty = "foo";
            await db.WaitForIdle();

            var value = File.ReadAllText(((ISexyDatabase)db).Node.PropertyNodes.Single().File.FullName);
            Assert.AreEqual(db.StringProperty, value);
        }

        public class StringPropertyDatabase : TestDatabase
        {
            public string StringProperty { get; set; }
        }
    }
}