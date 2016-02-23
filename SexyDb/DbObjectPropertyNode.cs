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
        }
    }
}