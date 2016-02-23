using System.IO;

namespace SexyDb
{
    public abstract class DbNode
    {
        public SexyDatabase Database { get; }
        public abstract DbNode EvaluatePath(string[] path, int index, bool returnLastNonNullNode = false);

        protected DbNode(SexyDatabase database)
        {
            Database = database;
        }

        public DbNode EvaluatePath(params string[] parts)
        {
            return EvaluatePath(parts, 0);
        }

        protected virtual void OnFileSystemChanged(FileSystemEventArgs args)
        {
        }

        internal void NotifyFileSystemChanged(FileSystemEventArgs args)
        {
            OnFileSystemChanged(args);
        }
    }
}