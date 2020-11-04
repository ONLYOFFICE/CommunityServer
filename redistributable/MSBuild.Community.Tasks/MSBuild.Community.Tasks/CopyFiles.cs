using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Copy directory.
    /// </summary>
    /// <example>Deploy website to folder.
    /// <code><![CDATA[
    /// <CopyDirectory 
    ///     SourceFolder="$(MSBuildProjectDirectory)" 
    ///     DestinationFolder="..\..\" 
    ///     ExcludeRegex="\.svn|/obj/|/Test/"
    ///     IncludeRegex=".*"
    ///     ReplaceFiles="true"
    /// />  
    /// ]]></code>
    /// </example>
    public class CopyFiles : Task
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public ITaskItem[] SourceFiles
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public ITaskItem[] DestinationFiles
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
                Parallel.ForEach(DestinationFiles.Select(r => Path.GetDirectoryName(r.ItemSpec)).Distinct().ToList(), (dir, state, arg3) =>
                {
                    if (!Directory.Exists(dir))
                    {
                        Log.LogMessage("Create dir '{0}'", dir);
                        Directory.CreateDirectory(dir);
                    }
                });

                Parallel.ForEach(SourceFiles, (item, state, arg3) =>
                {
                    var source = item.ItemSpec;
                    var destPath = DestinationFiles[arg3].ItemSpec;
                    if (!File.Exists(destPath))
                    {
                        Log.LogMessage("Copy file from '{0}' to '{1}'", source, destPath);
                        File.Copy(source, destPath);
                    }
                });

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

