using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using SexyReact;

namespace SexyDb
{
    public class DbObjectNode : DbNode
    {
        public IRxObject Object { get; }
        public DbObjectMetaData MetaData { get; }
        public ImmutableDictionary<string, DbPropertyNode> PropertyNodes { get; }
        public DirectoryInfo Directory { get; }

        public DbObjectNode(SexyDatabase database, IRxObject obj, DirectoryInfo directory) : base(database)
        {
            if (!directory.Exists)
            {
                directory.Create();
                directory.Refresh();
            }

            Object = obj;
            MetaData = DbObjectMetaDataCache.GetMetaData(obj.GetType());
            Directory = directory;
            PropertyNodes = MetaData.Properties.Select(x => CreateNode(x, obj)).ToImmutableDictionary(x => x.MetaData.Name);
        }

        public override DbNode EvaluatePath(string[] parts, int index)
        {
            DbPropertyNode property;
            if (PropertyNodes.TryGetValue(parts[index], out property))
            {
                if (index < parts.Length - 1)
                    return property.EvaluatePath(parts, index + 1);
                else
                    return property;
            }
            else
            {
                return null;
            }
        }

        private DbPropertyNode CreateNode(DbPropertyMetaData metaData, IRxObject container)
        {
            switch (metaData.Type)
            {
                case DbPropertyType.Value:
                    return new DbValuePropertyNode(Database, metaData, container, new FileInfo(Path.Combine(Directory.FullName, metaData.Name)));
                case DbPropertyType.Object:
                    return new DbObjectPropertyNode(Database, metaData, container, new DirectoryInfo(Path.Combine(Directory.FullName, metaData.Name)));
                case DbPropertyType.ObjectList:
                    return new DbObjectListPropertyNode(Database, metaData, container, new DirectoryInfo(Path.Combine(Directory.FullName, metaData.Name)));
                default:
                    throw new Exception();
            }
        }
    }
}