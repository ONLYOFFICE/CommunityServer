#region Copyright © 2010 MSBuild Community Task Project. All rights reserved.

/*
Copyright © 2008 MSBuild Community Task Project. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using Ionic.Zlib;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MSBuild.Community.Tasks.Properties;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Create a zip file with the files specified.
    /// </summary>
    /// <example>Create a zip file
    /// <code><![CDATA[
    /// <ItemGroup>
    ///     <ZipFiles Include="**\*.*" Exclude="*.zip" />
    /// </ItemGroup>
    /// <Target Name="Zip">
    ///     <Zip Files="@(ZipFiles)" 
    ///         ZipFileName="MSBuild.Community.Tasks.zip" />
    /// </Target>
    /// ]]></code>
    /// Create a zip file using a working directory.
    /// <code><![CDATA[
    /// <ItemGroup>
    ///     <RepoFiles Include="D:\svn\repo\**\*.*" />
    /// </ItemGroup>
    /// <Target Name="Zip">
    ///     <Zip Files="@(RepoFiles)" 
    ///         WorkingDirectory="D:\svn\repo" 
    ///         ZipFileName="D:\svn\repo.zip" />
    /// </Target>
    /// ]]></code>
    /// </example>
    public class Zip : Task
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Zip"/> class.
        /// </summary>
        public Zip()
        {
            ZipLevel = 6;
        }

        #endregion Constructor

        #region Input Parameters

        /// <summary>
        /// Gets or sets the name of the zip file.
        /// </summary>
        /// <value>The name of the zip file.</value>
        [Required]
        public string ZipFileName { get; set; }

        /// <summary>
        /// Gets or sets the zip level. Default is 6.
        /// </summary>
        /// <value>The zip level.</value>
        /// <remarks>0 - store only to 9 - means best compression</remarks>
        public int ZipLevel { get; set; }

        /// <summary>
        /// Gets or sets the files to zip.
        /// </summary>
        /// <value>The files to zip.</value>
        [Required]
        public ITaskItem[] Files { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Zip"/> is flatten.
        /// </summary>
        /// <value><c>true</c> if flatten; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Flattening the zip means that all directories will be removed 
        /// and the files will be place at the root of the zip file
        /// </remarks>
        public bool Flatten { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>The comment.</value>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the working directory for the zip file.
        /// </summary>
        /// <value>The working directory.</value>
        /// <remarks>
        /// The working directory is the base of the zip file.  
        /// All files will be made relative from the working directory.
        /// </remarks>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the encryption algorithm.
        /// </summary>
        /// <value>The encryption algorithm.</value>
        /// <remarks>
        /// Possible values are None, PkzipWeak, WinZipAes128 and WinZipAes256
        /// </remarks>
        public string Encryption { get; set; }

        #endregion Input Parameters

        #region Task Overrides

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            return ZipFiles();
        }

        #endregion Task Overrides

        #region Private Methods

        private bool ZipFiles()
        {
            try
            {
                Log.LogMessage(Resources.ZipCreating, ZipFileName);

                string directoryName = Path.GetDirectoryName(Path.GetFullPath(ZipFileName));
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);

                using (var zip = new ZipFile())
                {
                    zip.AlternateEncoding = System.Text.Encoding.Unicode;
                    zip.AlternateEncodingUsage = ZipOption.AsNecessary;

                    // make sure level in range
                    ZipLevel = System.Math.Max(0, ZipLevel);
                    ZipLevel = System.Math.Min(9, ZipLevel);
                    zip.CompressionLevel = (CompressionLevel)ZipLevel;

                    if (!string.IsNullOrEmpty(Password))
                        zip.Password = Password;

                    if (string.Equals(Encryption, "PkzipWeak", StringComparison.OrdinalIgnoreCase))
                        zip.Encryption = EncryptionAlgorithm.PkzipWeak;
                    else if (string.Equals(Encryption, "WinZipAes128", StringComparison.OrdinalIgnoreCase))
                        zip.Encryption = EncryptionAlgorithm.WinZipAes128;
                    else if (string.Equals(Encryption, "WinZipAes256", StringComparison.OrdinalIgnoreCase))
                        zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                    else
                        zip.Encryption = EncryptionAlgorithm.None;

                    if (!string.IsNullOrEmpty(Comment))
                        zip.Comment = Comment;

                    foreach (ITaskItem fileItem in Files)
                    {
                        string name = fileItem.ItemSpec;
                        string directoryPathInArchive;

                        // clean up name
                        if (Flatten)
                            directoryPathInArchive = string.Empty;
                        else if (!string.IsNullOrEmpty(WorkingDirectory))
                            directoryPathInArchive = GetPath(name, WorkingDirectory);
                        else
                            directoryPathInArchive = null;

                        if (!File.Exists(name))
                        {
                            // maybe a directory
                            if (Directory.Exists(name))
                            {
                                var directoryEntry = zip.AddDirectory(name, directoryPathInArchive);
                                Log.LogMessage(Resources.ZipAdded, directoryEntry.FileName);

                                continue;
                            }

                            Log.LogWarning(Resources.FileNotFound, name);
                            continue;
                        }

                        //remove file name
                        if (!string.IsNullOrEmpty(directoryPathInArchive)
                            && Path.GetFileName(directoryPathInArchive) == Path.GetFileName(name))
                            directoryPathInArchive = Path.GetDirectoryName(directoryPathInArchive);

                        var entry = zip.AddFile(name, directoryPathInArchive);
                        Log.LogMessage(Resources.ZipAdded, entry.FileName);
                    }

                    zip.Save(ZipFileName);
                    Log.LogMessage(Resources.ZipSuccessfully, ZipFileName);
                }
            }
            catch (Exception exc)
            {
                Log.LogErrorFromException(exc);
                return false;
            }

            return true;
        }

        private static string GetPath(string originalPath, string rootDirectory)
        {
            var relativePath = new List<string>();
            string[] originalDirectories = originalPath.Split(Path.DirectorySeparatorChar);
            string[] rootDirectories = rootDirectory.Split(Path.DirectorySeparatorChar);

            int length = System.Math.Min(originalDirectories.Length, rootDirectories.Length);

            int lastCommonRoot = -1;

            // find common root  
            for (int x = 0; x < length; x++)
            {
                if (!string.Equals(originalDirectories[x], rootDirectories[x],
                                   StringComparison.OrdinalIgnoreCase))
                    break;

                lastCommonRoot = x;
            }
            if (lastCommonRoot == -1)
                return originalPath;

            // add extra original directories
            for (int x = lastCommonRoot + 1; x < originalDirectories.Length; x++)
                relativePath.Add(originalDirectories[x]);

            return string.Join(Path.DirectorySeparatorChar.ToString(), relativePath.ToArray());
        }

        #endregion Private Methods
    }
}