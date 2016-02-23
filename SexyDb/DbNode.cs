using System.IO;

namespace SexyDb
{
    public abstract class DbNode
    {
        public SexyDatabase Database { get; }
        public abstract DbNode EvaluatePath(string[] path, int index);

        protected DbNode(SexyDatabase database)
        {
            Database = database;
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