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
        public static Task EditFile(this SexyDatabase db, FileInfo file, string value)
        {
            var completionSource = new TaskCompletionSource<object>();

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

        public static Task CreateDirectory(this SexyDatabase db, DirectoryInfo directory)
        {
            var completionSource = new TaskCompletionSource<object>();

            Action<IPropertyChanged> globalChanged = null;
            globalChanged = changed =>
            {
                db.GlobalChanged -= globalChanged;
                completionSource.SetResult(null);
            };
            db.GlobalChanged += globalChanged;
            directory.Create();
            return completionSource.Task.ContinueWith(async => db.WaitForIdle());
        } 
    }
}