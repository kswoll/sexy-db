// <copyright file="DatabaseExtensions.cs" company="PlanGrid, Inc.">
//     Copyright (c) 2016 PlanGrid, Inc. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading.Tasks;
using SexyReact;

namespace SexyDb.Tests
{
    public static class DatabaseExtensions
    {
        public static Task EditFile(this SexyDatabase db, FileInfo file, string value, Func<string> getPropertyValue = null)
        {
            var completionSource = new TaskCompletionSource<object>();
/*
            FileSystemEventHandler listener = null;
            listener = (sender, args) =>
            {
                var complete = false;
                lock (locker)
                {
                    var propertyValue = getPropertyValue?.Invoke();
                    if (args.FullPath == file.FullName && File.Exists(args.FullPath))
                    {
                        Console.WriteLine($"Completed: Property value: {propertyValue}, File value: {value}");
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
*/
            Action<IPropertyChanged> globalChanged = null;
            globalChanged = changed =>
            {
                db.GlobalChanged -= globalChanged;
                completionSource.SetResult(null);
            };
            db.GlobalChanged += globalChanged;
            File.WriteAllText(file.FullName, value);
            return completionSource.Task.ContinueWith(async => db.WaitForIdle());
        } 
    }
}