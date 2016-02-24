using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SexyReact;

namespace SexyDb.Tests
{
    [TestFixture]
    public class ObjectListPropertyTests
    {
        [Test]
        public async Task InitializedList()
        {
            var db = new ListDatabase();
            db.ListObject = new ListObject
            {
                Items = new RxList<ListItem> { new ListItem { StringProperty = "foo" } }
            };
            await db.WaitForIdle();

            var stringNode = (DbValuePropertyNode)((ISexyDatabase)db).Node.EvaluatePath(nameof(ListDatabase.ListObject), nameof(ListObject.Items), "0", nameof(ListItem.StringProperty));
            var value = File.ReadAllText(stringNode.File.FullName);
            Assert.AreEqual(db.ListObject.Items[0].StringProperty, value);
        }

        [Test]
        public async Task LoadItem()
        {
            var db = new ListDatabase();
            db.ListObject = new ListObject
            {
                Items = new RxList<ListItem>()
            };
            await db.WaitForIdle();

            var listNode = (DbObjectListPropertyNode)((ISexyDatabase)db).Node.EvaluatePath(nameof(ListDatabase.ListObject), nameof(ListObject.Items));
            var itemDirectory = Path.Combine(listNode.Directory.FullName, "0");
            Directory.CreateDirectory(itemDirectory);
            var value = "foo";
            var stringFile = new FileInfo(Path.Combine(itemDirectory, nameof(ListItem.StringProperty)));
            await db.EditFile(stringFile, value);

            Assert.AreEqual(value, db.ListObject.Items[0].StringProperty);
        }

        public class ListDatabase : TestDatabase
        {
            public ListObject ListObject { get; set; }
        }

        public class ListObject : BaseTestObject
        {
            public RxList<ListItem> Items { get; set; }            
        }

        public class ListItem : BaseTestObject
        {
            public string StringProperty { get; set; }
        }
    }
}