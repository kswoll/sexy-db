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

        private object locker = new object();
        private bool isSuppressingChange;

        public DbValuePropertyNode(SexyDatabase database, DbPropertyMetaData metaData, IRxObject container, FileInfo file) : base(database, metaData, container)
        {
            File = file;

            var isSavePending = false;
            container.GetChangedByProperty(metaData.Property)
                .Do(x => Database.StartAction(() => !isSavePending && (isSavePending = true)))
                .Throttle(TimeSpan.FromSeconds(.5))
                .Subscribe(x =>
                {
                    OnChanged(x);
                    database.FinishAction(() => isSavePending = false);
                });

            if (File.Exists)
                Load();
            else 
                System.IO.File.WriteAllText(file.FullName, "");

            var newValue = metaData.Property.GetValue(container, null);
            if (newValue != metaData.DefaultValue)
            {
                OnChanged(new PropertyChanged<object>(metaData.Property, null, newValue));
            }
        }

        public override DbNode EvaluatePath(string[] path, int index, bool returnLastNonNullNode = false)
        {
            throw new InvalidOperationException($"Cannot evaluate a subpath on a value property");
        }

        protected override void OnFileSystemChanged(FileSystemEventArgs args)
        {
            base.OnFileSystemChanged(args);

            Database.Action(() =>
            {
                var lastWriteTime = File.LastWriteTime;
                File.Refresh();
                if (System.IO.File.Exists(args.FullPath) && (args.ChangeType == WatcherChangeTypes.Created || File.LastWriteTime != lastWriteTime))
                {
                    Load();
                }                
            });
        }

        private void Load()
        {
            lock (locker)
            {
                isSuppressingChange = true;

                var text = System.IO.File.ReadAllText(File.FullName);
                var value = TypeConverter.Convert(text, MetaData.Property.PropertyType);
                MetaData.Property.SetValue(Container, value);

                isSuppressingChange = false;
            }
        }

        private void OnChanged(IPropertyChanged changed)
        {
            lock (locker)
            {
                if (isSuppressingChange)
                    return;
            }

            var value = changed.NewValue;
            if (value == null)
                File.Delete();
            else
                System.IO.File.WriteAllText(File.FullName, (string)TypeConverter.Convert(changed.NewValue, typeof(string)));
            Database.NotifyGlobalChanged(changed);
        }
    }
}