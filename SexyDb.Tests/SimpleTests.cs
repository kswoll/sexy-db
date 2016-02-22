using System.Diagnostics;
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

        [Test]
        public async Task LoadStringProperty()
        {
            var db = new StringPropertyDatabase();
            db.StringProperty = "foo";
            await db.WaitForIdle();

            await ((ISexyDatabase)db).Node.PropertyNodes.Single().File.Edit("bar");

            Assert.AreEqual("bar", db.StringProperty);
        }

        public class StringPropertyDatabase : TestDatabase
        {
            public string StringProperty { get; set; }
        }

        [Test]
        public async Task SaveIntProperty()
        {
            var db = new IntPropertyDatabase();
            db.IntProperty = 5;
            await db.WaitForIdle();

            var value = int.Parse(File.ReadAllText(((ISexyDatabase)db).Node.PropertyNodes.Single().File.FullName));
            Assert.AreEqual(db.IntProperty, value);
        }

        public class IntPropertyDatabase : TestDatabase
        {
            public int IntProperty { get; set; }
        }
    }
}