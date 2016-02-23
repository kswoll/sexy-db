// <copyright file="SexyDatabase.cs" company="PlanGrid, Inc.">
//     Copyright (c) 2016 PlanGrid, Inc. All rights reserved.
// </copyright>

using System.IO;
using System.Threading.Tasks;
using Nito.AsyncEx;
using SexyReact;

namespace SexyDb
{
    public class SexyDatabase : RxObject, ISexyDatabase
    {
        private readonly DbObjectNode node;

        internal readonly AsyncLock locker = new AsyncLock();
        internal readonly AsyncAutoResetEvent idle = new AsyncAutoResetEvent(false);
        internal int isSavePending;

        public SexyDatabase(string folder)
        {
            node = new DbObjectNode(this, this, new DirectoryInfo(folder));

            var fileSystemWatcher = new FileSystemWatcher(folder, "*.*");
            fileSystemWatcher.Changed += FileSystemChanged;
            fileSystemWatcher.Deleted += FileSystemChanged;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void FileSystemChanged(object sender, FileSystemEventArgs e)
        {
            var relativePath = e.FullPath.Substring(node.Directory.FullName.Length).TrimStart(Path.DirectorySeparatorChar);
            if (relativePath.Length > 0)
            {
                var targetNode = node.EvaluatePath(relativePath.Split(Path.DirectorySeparatorChar), 0);
                targetNode?.NotifyFileSystemChanged(e);
            }
        }

        DbObjectNode ISexyDatabase.Node => node;

        public async Task WaitForIdle()
        {
            using (await locker.LockAsync())
            {
                if (isSavePending == 0)
                    return;
            }
            await idle.WaitAsync();
        }
    }
}