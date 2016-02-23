using System;
using System.Collections.Generic;
using System.IO;
using SexyReact;

namespace SexyDb
{
    public class DbObjectListPropertyNode : DbPropertyNode
    {
        public DirectoryInfo Directory { get; }
        public List<DbObjectNode> ItemNodes { get; }

        private readonly object locker = new object();

        public DbObjectListPropertyNode(SexyDatabase database, DbPropertyMetaData metaData, IRxObject container, DirectoryInfo directory) : base(database, metaData, container)
        {
            Directory = directory;
            ItemNodes = new List<DbObjectNode>();

            var list = (IRxList)metaData.Property.GetValue(container, null);
            if (list == null)
                throw new ArgumentException($"{metaData.Property.DeclaringType.FullName}.{metaData.Property.Name} cannot be null");

            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                ItemNodes.Add(new DbObjectNode(database, (IRxObject)item, new DirectoryInfo(Path.Combine(Directory.FullName, i.ToString()))));
            }

            list.Changed.Subscribe(x => OnChanged(x));
        }

        public override DbNode EvaluatePath(string[] path, int index)
        {
            lock (locker)
            {
                int itemIndex;
                if (int.TryParse(path[index], out itemIndex) && itemIndex >= 0 && itemIndex < ItemNodes.Count)
                {
                    return ItemNodes[itemIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        private void OnChanged(RxListChange<object> changes)
        {
            lock (locker)
            {
                foreach (var added in changes.Added)
                {
                    for (var i = ItemNodes.Count - 1; i >= added.Index; i--)
                    {
                        ItemNodes[i].Directory.MoveTo(Path.Combine(Directory.FullName, i.ToString()));
                    }
                    ItemNodes.Insert(added.Index, new DbObjectNode(Database, (IRxObject)added.Value, new DirectoryInfo(Path.Combine(Directory.FullName, added.Value.ToString()))));
                }                
            }
        }
    }
}