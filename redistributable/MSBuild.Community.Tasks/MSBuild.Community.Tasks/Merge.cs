#region Copyright © 2005 Paul Welter. All rights reserved.
/*
Copyright © 2005 Paul Welter. All rights reserved.

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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using MSBuild.Community.Tasks.Properties;



namespace MSBuild.Community.Tasks
{
    #region Enums
    /// <summary>
    /// Defines the modes for merging files.
    /// </summary>
    public enum MergeModes
    {
        /// <summary>
        /// Merges files as binary data.
        /// </summary>
        Binary,

        /// <summary>
        /// Merges files as text.
        /// </summary>
        Text,

        /// <summary>
        /// Merges files as text line by line.
        /// </summary>
        TextLine
    }
    #endregion Enums

    /// <summary>
    /// Merge files into the destination file.
    /// </summary>
    /// <example>Merge CSS files together for better browser performance.
    /// <code><![CDATA[
    /// <Merge Mode="TextLine" 
    ///     SourceFiles="Main.css;Login.css" 
    ///     DestinationFile="All.css" />
    /// ]]></code>
    /// </example>
    public class Merge : Task
    {
        private const int bufferSize = 32768;

        private MergeModes mode = MergeModes.Binary;

        /// <summary>
        /// Gets or sets the mode to use when merging.
        /// </summary>
        /// <value>The merge mode.</value>
        /// <enum cref="MSBuild.Community.Tasks.MergeModes"/>
        public string Mode
        {
            get { return mode.ToString(); }
            set { mode = (MergeModes)Enum.Parse(typeof(MergeModes), value); }
        }

        private ITaskItem[] sourceFiles;

        /// <summary>
        /// Gets or sets the source files to merge.
        /// </summary>
        /// <value>The source files to merge.</value>
        [Required]
        public ITaskItem[] SourceFiles
        {
            get { return this.sourceFiles; }
            set { this.sourceFiles = value; }
        }

        private ITaskItem destinationFile;

        /// <summary>
        /// Gets or sets the destination file where the
        /// <see cref="SourceFiles"/> are merged to.
        /// </summary>
        /// <value>The destination file.</value>
        [Required]
        public ITaskItem DestinationFile
        {
            get { return destinationFile; }
            set { destinationFile = value; }
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            if (sourceFiles == null || sourceFiles.Length == 0)
            {
                Log.LogMessage(MessageImportance.Normal,
                        Resources.MergeCompleteNoSourceFiles, 0, destinationFile?.ItemSpec);

                return true;
            }

            bool result = false;

            switch (mode)
            {
                case MergeModes.Text:
                    TextMerge();
                    break;
                case MergeModes.TextLine:
                    TextLineMerge();
                    break;
                default:
                    BinaryMerge();
                    break;
            }

            Log.LogMessage(MessageImportance.Normal,
                        Resources.MergeComplete,
                        sourceFiles.Length, destinationFile.ItemSpec);

            result = true;

            return result;
        }

        private void BinaryMerge()
        {
            byte[] buffer = new byte[bufferSize];

            using (FileStream w = File.Create(
                destinationFile.ItemSpec, bufferSize, FileOptions.None))
            {
                foreach (ITaskItem item in SourceFiles)
                {
                    Log.LogMessage(MessageImportance.Normal,
                        Resources.MergingFile,
                        item.ItemSpec, destinationFile.ItemSpec);

                    using (FileStream r = File.OpenRead(item.ItemSpec))
                    {
                        long len = r.Length;
                        while (len > 0)
                        {
                            int readSoFar = r.Read(buffer, 0, buffer.Length);
                            w.Write(buffer, 0, readSoFar);
                            len -= readSoFar;
                        }
                    }
                }
                w.Flush();
                w.Close();
            }
        }

        private void TextMerge()
        {
            using (StreamWriter w = File.CreateText(destinationFile.ItemSpec))
            {
                foreach (ITaskItem item in SourceFiles)
                {
                    Log.LogMessage(MessageImportance.Normal,
                        Resources.MergingFile,
                        item.ItemSpec, destinationFile.ItemSpec);

                    w.Write(File.ReadAllText(item.ItemSpec));
                }
                w.Flush();
                w.Close();
            }
        }

        private void TextLineMerge()
        {
            using (StreamWriter w = File.CreateText(destinationFile.ItemSpec))
            {
                foreach (ITaskItem item in SourceFiles)
                {
                    Log.LogMessage(MessageImportance.Normal,
                        Resources.MergingFile,
                        item.ItemSpec, destinationFile.ItemSpec);

                    string[] lines = File.ReadAllLines(item.ItemSpec);
                    foreach (string line in lines)
                        w.WriteLine(line);
                }
                w.Flush();
                w.Close();
            }
        }
    }
}
