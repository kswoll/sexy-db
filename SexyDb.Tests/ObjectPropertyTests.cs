using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SexyReact;

namespace SexyDb.Tests
{
    [TestFixture]
    public class ObjectPropertyTests
    {
        [Test]
        public void ObjectPropertyCreatesFolder()
        {
            var db = new ObjectPropertyDatabase();
            db.Object = new TestObject();

            Assert.IsTrue(((ISexyDatabase)db).Node.PropertyNodes.Values.Cast<DbObjectPropertyNode>().Single().Directory.Exists);
        }

        [Test]
        public void NullingOutObjectPropertyDeletesFolder()
        {
            var db = new ObjectPropertyDatabase();
            db.Object = new TestObject();
            db.Object = null;

            Assert.IsFalse(((ISexyDatabase)db).Node.PropertyNodes.Values.Cast<DbObjectPropertyNode>().Single().Directory.Exists);
        }

        [Test]
        public async Task SubpropertyIsSaved()
        {
            var db = new ObjectPropertyDatabase();
            db.Object = new TestObject();
            db.Object.StringProperty = "foo";

            await db.WaitForIdle();

            Assert.AreEqual("foo", File.ReadAllText(((ISexyDatabase)db).Node.PropertyNodes.Values.Cast<DbObjectPropertyNode>().Single().Object.PropertyNodes.Values.Cast<DbValuePropertyNode>().Single().File.FullName));
        }

        [Test]
        public async Task SubpropertyIsLoaded()
        {
            var db = new ObjectPropertyDatabase();
            db.Object = new TestObject();
            db.Object.StringProperty = "foo";

            await db.WaitForIdle();

            await ((ISexyDatabase)db).Node.PropertyNodes.Values.Cast<DbObjectPropertyNode>().Single().Object.PropertyNodes.Values.Cast<DbValuePropertyNode>().Single().File.Edit("bar");

            Assert.AreEqual("bar", db.Object.StringProperty);
        }

        [Test]
        public async Task InitialValueSaved()
        {
            var db = new ObjectPropertyDatabase();
            db.Object = new TestObject { StringProperty = "foo" };

            await db.WaitForIdle();

            Assert.AreEqual("foo", File.ReadAllText(((ISexyDatabase)db).Node.PropertyNodes.Values.Cast<DbObjectPropertyNode>().Single().Object.PropertyNodes.Values.Cast<DbValuePropertyNode>().Single().File.FullName));            
        }

        public class ObjectPropertyDatabase : TestDatabase
        {
            public TestObject Object { get; set; }
        }

        public class TestObject : BaseTestObject
        {
            public string StringProperty { get; set; }
        }
    }
}