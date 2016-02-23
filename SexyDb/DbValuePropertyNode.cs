using System;
using System.IO;
using System.Reactive.Linq;
using SexyDb.TypeConverters;
using SexyReact;

namespace SexyDb
{
    public class DbValuePropertyNode : DbPropertyNode
    {
        public FileInfo File { get; }

        public DbValuePropertyNode(SexyDatabase database, DbPropertyMetaData metaData, IRxObject container, FileInfo file) : base(database, metaData, container)
        {
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

            OnChanged(new PropertyChanged<object>(metaData.Property, null, metaData.Property.GetValue(container, null)));
        }

        public override DbNode EvaluatePath(string[] path, int index)
        {
            throw new InvalidOperationException($"Cannot evaluate a subpath to a value property");
        }

        protected override void OnFileSystemChanged(FileSystemEventArgs args)
        {
            base.OnFileSystemChanged(args);

            var lastWriteTime = File.LastWriteTime;
            File.Refresh();
            if (File.LastWriteTime != lastWriteTime)
            {
                var text = System.IO.File.ReadAllText(File.FullName);
                var value = TypeConverter.Convert(text, MetaData.Property.PropertyType);
                MetaData.Property.SetValue(Container, value);                
            }
        }

        private void OnChanged(IPropertyChanged changed)
        {
            var value = changed.NewValue;
            if (value == null)
                File.Delete();
            else
                System.IO.File.WriteAllText(File.FullName, (string)TypeConverter.Convert(changed.NewValue, typeof(string)));
        }
    }
}