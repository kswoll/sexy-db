using System;
using System.IO;
using System.Reactive.Linq;
using SexyDb.TypeConverters;
using SexyReact;

namespace SexyDb
{
    public class DbPropertyNode : DbNode
    {
        public DbObjectPropertyMetaData MetaData { get; }
        public IRxObject Container { get; }
        public FileInfo File { get; }

        public DbPropertyNode(SexyDatabase database, DbObjectPropertyMetaData metaData, IRxObject container, FileInfo file) : base(database)
        {
            MetaData = metaData;
            Container = container;
            File = file;

            var isSavePending = false;
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
            file.Create().Close();
            var fileSystemWatcher = new FileSystemWatcher(file.DirectoryName, file.Name);
            fileSystemWatcher.Changed += SourceChanged;
            fileSystemWatcher.EnableRaisingEvents = true;
            Console.WriteLine();
        }

        private void SourceChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            var text = System.IO.File.ReadAllText(File.FullName);
            var value = TypeConverter.Convert(text, MetaData.Property.PropertyType);
            MetaData.Property.SetValue(Container, value);
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