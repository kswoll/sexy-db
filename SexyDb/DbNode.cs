namespace SexyDb
{
    public class DbNode
    {
        public SexyDatabase Database { get; }

        public DbNode(SexyDatabase database)
        {
            Database = database;
        }
    }
}