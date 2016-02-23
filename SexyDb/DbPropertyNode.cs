using SexyReact;

namespace SexyDb
{
    public abstract class DbPropertyNode : DbNode
    {
        public DbPropertyMetaData MetaData { get; }
        public IRxObject Container { get; }

        protected DbPropertyNode(SexyDatabase database, DbPropertyMetaData metaData, IRxObject container) : base(database)
        {
            MetaData = metaData;
            Container = container;
        }
    }
}