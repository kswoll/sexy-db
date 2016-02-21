using System.Collections.Immutable;
using System.IO;
using System.Linq;
using SexyReact;

namespace SexyDb
{
    public class DbObjectNode : DbNode
    {
        public DbObjectMetaData MetaData { get; }
        public ImmutableList<DbPropertyNode> PropertyNodes { get; }

        public DbObjectNode(SexyDatabase database, IRxObject obj, DirectoryInfo directory) : base(database)
        {
            if (!directory.Exists)
                directory.Create();

            MetaData = DbObjectMetaDataCache.GetMetaData(obj.GetType());
            PropertyNodes = MetaData.Properties.Select(x => new DbPropertyNode(database, x, obj, new FileInfo(Path.Combine(directory.FullName, x.Name)))).ToImmutableList();
        }
    }
}