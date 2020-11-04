using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    public class CopyDirectory : Task
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
        [Required]
        public string DestinationFolder
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ExcludeRegex
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string IncludeRegex
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ReplaceFiles
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
                Log.LogMessage("Copy directory from '{0}' to '{1}'", SourceFolder, DestinationFolder);

                if (Path.DirectorySeparatorChar == '/')
                {
                    if (!string.IsNullOrEmpty(IncludeRegex))
                    {
                        IncludeRegex = IncludeRegex.Replace('/', '\\').Replace(@"\\", "/");
                        Log.LogMessage("Include regex: {0}", IncludeRegex);
                    }
                    if (!string.IsNullOrEmpty(ExcludeRegex))
                    {
                        ExcludeRegex = ExcludeRegex.Replace('/', '\\').Replace(@"\\", "/");
                        Log.LogMessage("Exclude regex: {0}", ExcludeRegex);
                    }
                }
                var include = string.IsNullOrEmpty(IncludeRegex) ? null : new Regex(IncludeRegex, RegexOptions.IgnoreCase);
                var exclude = string.IsNullOrEmpty(ExcludeRegex) ? null : new Regex(ExcludeRegex, RegexOptions.IgnoreCase);

                var fullSource = Path.GetFullPath(SourceFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                var fullDest = Path.GetFullPath(DestinationFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

                var sources = new List<string>();
                var dirs = Directory.GetDirectories(fullSource, "*", SearchOption.AllDirectories);
                for (var i = 0; i < dirs.Length; i++)
                {
                    dirs[i] = dirs[i] + Path.DirectorySeparatorChar;
                }
                sources.AddRange(dirs);
                sources.AddRange(Directory.GetFiles(fullSource, "*", SearchOption.AllDirectories));
                
                Parallel.ForEach(sources, source =>
                {
                    var fileRelative = Path.DirectorySeparatorChar + source.Substring(fullSource.Length);

                    if ((include != null && !include.IsMatch(fileRelative)) ||
                        (exclude != null && exclude.IsMatch(fileRelative)))
                    {
                        return;
                    }

                    var destPath = Path.Combine(fullDest, fileRelative.Trim(Path.DirectorySeparatorChar));
                    if (Directory.Exists(source))
                    {
                        Log.LogMessage("Create directory '{0}'", destPath);
                        Directory.CreateDirectory(destPath);
                    }
                    else if (!File.Exists(destPath) || ReplaceFiles)
                    {
                        Log.LogMessage("Copy file from '{0}' to '{1}'", source, destPath);
                        var dir = Path.GetDirectoryName(destPath);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
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
