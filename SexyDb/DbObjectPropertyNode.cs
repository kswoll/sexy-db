using System;
using System.IO;
using SexyDb.TypeConverters;
using SexyDb.Utils;
using SexyReact;

namespace SexyDb
{
    public class DbObjectPropertyNode : DbPropertyNode
    {
        public DirectoryInfo Directory { get; }
        public DbObjectNode Object { get; set;  }

        private readonly object locker = new object();
        private bool isSuppressingChange;

        public DbObjectPropertyNode(SexyDatabase database, DbPropertyMetaData metaData, IRxObject container, DirectoryInfo directory) : base(database, metaData, container)
        {
            Directory = directory;

            container.GetChangedByProperty(metaData.Property).Subscribe(OnChanged);
        }

        public override DbNode EvaluatePath(string[] path, int index, bool returnLastNonNullNode = false)
        {
            return Object?.EvaluatePath(path, index, returnLastNonNullNode);
        }

        private void OnChanged(IPropertyChanged changed)
        {
            lock (locker)
            {
                if (isSuppressingChange)
                    return;
                isSuppressingChange = true;
            }

            var value = changed.NewValue;
            if (value == null)
            {
                Object = null;
                Directory.DeleteTree();
                Directory.Refresh();
            }
            else
            {
                Object = new DbObjectNode(Database, (IRxObject)changed.NewValue, Directory);
            }

            lock (locker)
            {
                isSuppressingChange = false;
            }             
        }

        private void Load()
        {
            lock (locker)
            {
                if (isSuppressingChange)
                    return;

                isSuppressingChange = true;
            }
            if (Object == null)
            {
                Database.Action(() =>
                {
                    var obj = (IRxObject)Activator.CreateInstance(MetaData.Property.PropertyType);
                    Object = new DbObjectNode(Database, obj, Directory);
                    MetaData.Property.SetValue(Container, obj);
                });
            }

            lock (locker)
            {
                isSuppressingChange = false;
            }            
        }

        protected override void OnFileSystemChanged(FileSystemEventArgs args)
        {
            base.OnFileSystemChanged(args);

            switch (args.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    Load();
                    break;
            }
        }
    }
}