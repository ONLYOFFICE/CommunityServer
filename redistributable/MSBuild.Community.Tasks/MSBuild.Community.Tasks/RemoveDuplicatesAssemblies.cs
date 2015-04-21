using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// 
    /// </summary>
    public class RemoveDuplicatesAssemblies : Task
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string SourceFolder
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            try
            {
                Log.LogMessage("Remove duplicates assemblies from '{0}'", SourceFolder);

                if (Path.DirectorySeparatorChar == '/')
                {
                    SourceFolder = SourceFolder.Replace('\\', Path.DirectorySeparatorChar);
                }
                var fullSource = Path.GetFullPath(SourceFolder).TrimEnd(Path.DirectorySeparatorChar);

                var mainBinFiles = Directory.GetFiles(Path.Combine(fullSource, "bin"), "*.dll", SearchOption.AllDirectories);
                foreach (var f in Directory.GetFiles(fullSource, "*.dll", SearchOption.AllDirectories))
                {
                    if (Array.Exists(mainBinFiles, (s) => f == s))
                    {
                        continue;
                    }
                    if (Array.Exists(mainBinFiles, (s) => Path.GetFileName(f) == Path.GetFileName(s)))
                    {
                        Log.LogMessage("Remove duplicate files '{0}'", f);
                        File.Delete(f);
                    }
                }

                return true;
            }
            catch (Exception err)
            {
                Log.LogErrorFromException(err);
                return false;
            }
        }
    }
}
