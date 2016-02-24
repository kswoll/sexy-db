// <copyright file="DatabaseExtensions.cs" company="PlanGrid, Inc.">
//     Copyright (c) 2016 PlanGrid, Inc. All rights reserved.
// </copyright>

using System.IO;
using System.Threading.Tasks;

namespace SexyDb.Tests
{
    public static class DatabaseExtensions
    {
        public static Task EditFile(this SexyDatabase db, FileInfo file, string value)
        {
            var completionSource = new TaskCompletionSource<object>();
            var locker = new object();
            FileSystemEventHandler listener = null;
            listener = (sender, args) =>
            {
                var complete = false;
                lock (locker)
                {
                    if (args.FullPath == file.FullName && File.Exists(args.FullPath))
                    {
                        complete = true;
                        db.FileSystemEvents -= listener;
                    }                    
                }
                if (complete)
                {
                    completionSource.SetResult(null);
                }
            };
            db.FileSystemEvents += listener;
            File.WriteAllText(file.FullName, value);
            return completionSource.Task.ContinueWith(async => db.WaitForIdle());
        } 
    }
}