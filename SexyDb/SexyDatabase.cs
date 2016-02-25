// <copyright file="SexyDatabase.cs" company="PlanGrid, Inc.">
//     Copyright (c) 2016 PlanGrid, Inc. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using SexyReact;

namespace SexyDb
{
    public class SexyDatabase : RxObject, ISexyDatabase
    {
        public event FileSystemEventHandler FileSystemEvents;
        public event Action<IPropertyChanged> GlobalChanged;

        private readonly DbObjectNode node;

        private readonly AsyncLock locker = new AsyncLock();
        private readonly AsyncAutoResetEvent idle = new AsyncAutoResetEvent(false);
        private int isActionPending;

        public SexyDatabase(string folder)
        {
            node = new DbObjectNode(this, this, new DirectoryInfo(folder));

            var fileSystemWatcher = new FileSystemWatcher(folder, "*.*");
            fileSystemWatcher.Changed += FileChanged;
            fileSystemWatcher.Created += FileExistance;
            fileSystemWatcher.Deleted += FileExistance;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        public void StartAction(Func<bool> predicate)
        {
            using (locker.Lock())
            {
                if (predicate())
                {
                    isActionPending++;
                }
            }
        }

        public void FinishAction(Action action)
        {
            using (locker.Lock())
            {
                action();
                isActionPending--;
                if (isActionPending == 0)
                    idle.Set();
            }            
        }

        public void Action(Action action)
        {
            StartAction(() => true);
            try
            {
                action();
            }
            finally
            {
                FinishAction(() => {});
            }
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            var relativePath = e.FullPath.Substring(node.Directory.FullName.Length).TrimStart(Path.DirectorySeparatorChar);
            if (relativePath.Length > 0)
            {
                var targetNode = node.EvaluatePath(relativePath.Split(Path.DirectorySeparatorChar), 0);
                targetNode?.NotifyFileSystemChanged(e);
                FileSystemEvents?.Invoke(sender, e);
            }
        }

        private void FileExistance(object sender, FileSystemEventArgs e)
        {
            var relativePath = e.FullPath.Substring(node.Directory.FullName.Length).TrimStart(Path.DirectorySeparatorChar);
            if (relativePath.Length > 0)
            {
                var parts = relativePath.Split(Path.DirectorySeparatorChar);
                var targetNode = node.EvaluatePath(parts, 0, true);
                targetNode?.NotifyFileSystemChanged(e);                    
                FileSystemEvents?.Invoke(sender, e);
            }
        }

        DbObjectNode ISexyDatabase.Node => node;

        public async Task WaitForIdle()
        {
            using (await locker.LockAsync())
            {
                if (isActionPending == 0)
                    return;
            }
            await idle.WaitAsync();
        }

        internal void NotifyGlobalChanged(IPropertyChanged changed)
        {
            OnGlobalChanged(changed);
        }

        protected void OnGlobalChanged(IPropertyChanged changed)
        {
            GlobalChanged?.Invoke(changed);
        }
    }
}