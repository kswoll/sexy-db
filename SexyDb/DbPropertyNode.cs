using SexyReact;

namespace SexyDb
{
    public abstract class DbPropertyNode : DbNode
    {
        public DbObjectPropertyMetaData MetaData { get; }
        public IRxObject Container { get; }

        public DbPropertyNode(SexyDatabase database, DbObjectPropertyMetaData metaData, IRxObject container) : base(database)
        {
            MetaData = metaData;
            Container = container;
        }
    }
}