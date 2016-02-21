// <copyright file="SexyDatabase.cs" company="PlanGrid, Inc.">
//     Copyright (c) 2016 PlanGrid, Inc. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
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