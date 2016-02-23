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
        public IRxList List { get; }

        private readonly object locker = new object();
        private DbChangeMode changeMode;

        public DbObjectListPropertyNode(SexyDatabase database, DbPropertyMetaData metaData, IRxObject container, DirectoryInfo directory) : base(database, metaData, container)
        {
            Directory = directory;
            ItemNodes = new List<DbObjectNode>();

            List = (IRxList)metaData.Property.GetValue(container, null);
            if (List == null)
                throw new ArgumentException($"{metaData.Property.DeclaringType.FullName}.{metaData.Property.Name} cannot be null");

            for (var i = 0; i < List.Count; i++)
            {
                var item = List[i];
                ItemNodes.Add(new DbObjectNode(database, (IRxObject)item, new DirectoryInfo(Path.Combine(Directory.FullName, i.ToString()))));
            }

            List.Changed.Subscribe(x => OnChanged(x));
        }

        public override DbNode EvaluatePath(string[] path, int index, bool returnLastNonNullNode = false)
        {
            lock (locker)
            {
                int itemIndex;
                if (int.TryParse(path[index], out itemIndex) && itemIndex >= 0 && itemIndex < ItemNodes.Count)
                {
                    var itemNode = ItemNodes[itemIndex];
                    if (index < path.Length - 1)
                        return itemNode.EvaluatePath(path, index + 1);
                    else
                        return itemNode;
                }
                else
                {
                    return returnLastNonNullNode ? this : null;
                }
            }
        }

        private void OnChanged(RxListChange<object> changes)
        {
            lock (locker)
            {
                if (changeMode == DbChangeMode.Load)
                    return;

                changeMode = DbChangeMode.Save;
                try
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
                finally
                {
                    changeMode = DbChangeMode.None;
                }
            }
        }

        protected override void OnFileSystemChanged(FileSystemEventArgs args)
        {
            base.OnFileSystemChanged(args);

            lock (locker)
            {
                changeMode = DbChangeMode.Load;
                try
                {
                    if (args.FullPath != Directory.FullName)
                    {
                        var relativePath = args.FullPath.Substring(Directory.FullName.Length).TrimStart(Path.DirectorySeparatorChar);
                        var parts = relativePath.Split(Path.DirectorySeparatorChar);
                        if (parts.Length == 1)
                        {
                            switch (args.ChangeType)
                            {
                                case WatcherChangeTypes.Created:
                                    int index;
                                    if (int.TryParse(relativePath, out index))
                                    {
                                        if (index >= ItemNodes.Count || ItemNodes[index] == null)
                                        {
                                            var obj = (IRxObject)Activator.CreateInstance(MetaData.ElementType);
                                            var node = new DbObjectNode(Database, obj, new DirectoryInfo(Path.Combine(Directory.FullName, index.ToString())));
                                            while (index > ItemNodes.Count)
                                                ItemNodes.Add(null);
                                            while (index > List.Count)
                                                List.Add(null);
                                            if (index < ItemNodes.Count)
                                                ItemNodes[index] = node;
                                            else
                                                ItemNodes.Add(node);
                                            if (index < List.Count)
                                                List[index] = obj;
                                            else
                                                List.Add(obj);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
                finally
                {
                    changeMode = DbChangeMode.None;
                }
            }
        }
    }
}