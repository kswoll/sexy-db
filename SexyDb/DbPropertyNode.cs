using System;
using System.IO;
using System.Reactive.Linq;
using SexyReact;

namespace SexyDb
{
    public class DbPropertyNode : DbNode
    {
        public SexyDatabase Database { get; }
        public DbObjectPropertyMetaData MetaData { get; }
        public IRxObject Container { get; }
        public FileInfo File { get; }

        public DbPropertyNode(SexyDatabase database, DbObjectPropertyMetaData metaData, IRxObject container, FileInfo file) : base(database)
        {
            Database = database;
            MetaData = metaData;
            Container = container;
            File = file;

            bool isSavePending = false;
            container.GetChangedByProperty(metaData.Property)
                .Do(x =>
                {
                    using (database.locker.Lock())
                    {
                        if (!isSavePending)
                        {
                            isSavePending = true;
                            database.isSavePending++;
                        }
                    }
                })
                .Throttle(TimeSpan.FromSeconds(.5))
                .Subscribe(x =>
                {
                    OnChanged(x);
                    using (database.locker.Lock())
                    {
                        database.isSavePending--;
                        isSavePending = false;
                        if (database.isSavePending == 0)
                            database.idle.Set();
                    }
                });
        }

        private void OnChanged(IPropertyChanged changed)
        {
            var value = changed.NewValue;
            if (value == null)
                File.Delete();
            else
                System.IO.File.WriteAllText(File.FullName, changed.NewValue.ToString());
        }
    }
}