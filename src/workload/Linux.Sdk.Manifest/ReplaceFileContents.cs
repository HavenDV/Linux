// https://github.com/xamarin/xamarin-android/blob/011a77134566b50ff48dfc4cad98ec7e07b333c2/build-tools/xa-prep-tasks/Xamarin.Android.BuildTools.PrepTasks/ReplaceFileContents.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Linux.Build.PrepTasks
{
    public class ReplaceFileContents : Task
    {
        [Required]
        public ITaskItem SourceFile { get; set; }

        [Required]
        public ITaskItem DestinationFile { get; set; }

        public string[] Replacements { get; set; }
        public string ReplacementFilePath { get; set; }

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.Low, $"Task {nameof(ReplaceFileContents)}");
            Log.LogMessage(MessageImportance.Low, $"  {nameof(SourceFile)}: {SourceFile.ItemSpec}");
            Log.LogMessage(MessageImportance.Low, $"  {nameof(DestinationFile)}: {DestinationFile.ItemSpec}");
            Log.LogMessage(MessageImportance.Low, $"  {nameof(Replacements)}:");
            if (Replacements != null)
            {
                foreach (var replacement in Replacements)
                {
                    Log.LogMessage(MessageImportance.Low, $"    {replacement}");
                }
            }

            if (File.Exists(DestinationFile.ItemSpec))
                File.Delete(DestinationFile.ItemSpec);

            string[] replacements;
            if (!String.IsNullOrEmpty(ReplacementFilePath))
                replacements = File.ReadAllLines(ReplacementFilePath);
            else
                replacements = Replacements;
            var r = GetReplacementInfo(replacements);
            using (var i = File.OpenText(SourceFile.ItemSpec))
            using (var o = File.CreateText(DestinationFile.ItemSpec))
            {
                string line;
                while ((line = i.ReadLine()) != null)
                {
                    foreach (var e in r)
                    {
                        line = line.Replace(e.Key, e.Value);
                    }
                    o.WriteLine(line);
                }
            }

            return !Log.HasLoggedErrors;
        }

        static readonly char[] Separator = new[] { '=' };

        static Dictionary<string, string> GetReplacementInfo(string[] replacements)
        {
            var r = new Dictionary<string, string>(replacements?.Length ?? 0);
            if (replacements == null)
                return r;
            foreach (var e in replacements)
            {
                if (string.IsNullOrEmpty(e))
                    continue;
                var kvp = e.Split(Separator, 2, StringSplitOptions.RemoveEmptyEntries);
                r.Add(kvp[0], kvp.Length > 1 ? kvp[1] : "");
            }
            return r;
        }
    }
}
