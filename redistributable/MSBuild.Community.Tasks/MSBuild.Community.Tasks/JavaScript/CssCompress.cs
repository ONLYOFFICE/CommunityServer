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
using System.Text.RegularExpressions;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// MSBuild task to minimize the size of a css file.
    /// </summary>
    public class CssCompress : Task
    {
        #region Properties
        // Properties
        private ITaskItem[] compressedFiles;

        /// <summary>
        /// Gets the items that were successfully compressed.
        /// </summary>
        /// <value>The compressed files.</value>
        [Output]
        public ITaskItem[] CompressedFiles
        {
            get { return this.compressedFiles; }
        }

        private ITaskItem[] destinationFiles;

        /// <summary>
        /// Gets or sets the list of files to compressed the source files to. 
        /// </summary>
        /// <remarks>
        /// This list is expected to be a one-to-one mapping with the 
        /// list specified in the SourceFiles parameter. That is, the 
        /// first file specified in SourceFiles will be compressed to the 
        /// first location specified in DestinationFiles, and so forth.
        /// </remarks>
        /// <value>The destination files.</value>
        [Output]
        public ITaskItem[] DestinationFiles
        {
            get { return this.destinationFiles; }
            set { this.destinationFiles = value; }
        }

        private ITaskItem destinationFolder;

        /// <summary>
        /// Gets or sets the directory to which you want to compress the files.
        /// </summary>
        /// <value>The destination folder.</value>
        public ITaskItem DestinationFolder
        {
            get { return this.destinationFolder; }
            set { this.destinationFolder = value; }
        }

        private ITaskItem[] sourceFiles;

        /// <summary>
        /// Gets or sets the source files to compress.
        /// </summary>
        /// <value>The source files to compress.</value>
        [Required]
        public ITaskItem[] SourceFiles
        {
            get { return this.sourceFiles; }
            set { this.sourceFiles = value; }
        }
        #endregion
        
        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            bool result = true;
            if ((this.sourceFiles == null) || (this.sourceFiles.Length == 0))
            {
                this.destinationFiles = new TaskItem[0];
                this.compressedFiles = new TaskItem[0];
                return true;
            }
            if ((this.destinationFiles == null) && (this.destinationFolder == null))
            {
                Log.LogError("No destination specified for compression. Please supply either '{0}' or '{1}'.", "DestinationFiles", "DestinationDirectory");
                return false;
            }
            if ((this.destinationFiles != null) && (this.destinationFolder != null))
            {
                Log.LogError(Resources.ExactlyOneTypeOfDestination, "DestinationFiles", "DestinationDirectory");
                return false;
            }
            if ((this.destinationFiles != null) && (this.destinationFiles.Length != this.sourceFiles.Length))
            {
                Log.LogError(Resources.TwoVectorsMustHaveSameLength,
                    this.destinationFiles.Length,
                    this.sourceFiles.Length,
                    "DestinationFiles",
                    "SourceFiles");
                return false;
            }

            // create destination array
            if (this.destinationFiles == null)
            {
                this.destinationFiles = new ITaskItem[this.sourceFiles.Length];
                for (int x = 0; x < this.sourceFiles.Length; x++)
                {
                    string newPath;
                    try
                    {
                        newPath = Path.Combine(this.destinationFolder.ItemSpec, Path.GetFileName(this.sourceFiles[x].ItemSpec));
                    }
                    catch (ArgumentException ex)
                    {
                        this.destinationFiles = new ITaskItem[0];
                        Log.LogError(Resources.MoveError,
                            this.sourceFiles[x].ItemSpec,
                            this.destinationFolder.ItemSpec,
                            ex.Message);
                        return false;
                    }
                    this.destinationFiles[x] = new TaskItem(newPath);
                    this.sourceFiles[x].CopyMetadataTo(this.destinationFiles[x]);
                }
            }

            ArrayList compressedList = new ArrayList();
            for (int x = 0; x < this.sourceFiles.Length; x++)
            {
                string sourceFile = this.sourceFiles[x].ItemSpec;
                string destinationFile = this.destinationFiles[x].ItemSpec;
                try
                {
                    if (Directory.Exists(destinationFile))
                    {
                        Log.LogError(Resources.TaskDestinationIsDirectory, sourceFile, destinationFile);
                        return false;
                    }
                    if (Directory.Exists(sourceFile))
                    {
                        Log.LogError(Resources.TaskSourceIsDirectory, sourceFile, "CssCompress");
                        return false;
                    }
                    
                    string buffer = File.ReadAllText(sourceFile);
                    buffer = Compress(buffer);
                    File.WriteAllText(destinationFile, buffer);

                    this.sourceFiles[x].CopyMetadataTo(this.destinationFiles[x]);
                    compressedList.Add(this.destinationFiles[x]);
                    
                }                
                catch (Exception ex)
                {
                    Log.LogError(Resources.MoveError,
                        sourceFile,
                        destinationFile,
                        ex.Message);
                    result = false;
                }
            }
            this.compressedFiles = (ITaskItem[])compressedList.ToArray(typeof(ITaskItem));
            return result;

        }

        private string Compress(string source)
        {
            string body = source;

            //body = body.Replace("  ", string.Empty);
            body = body.Replace(Environment.NewLine, string.Empty);
            body = body.Replace("\n", string.Empty);
            body = body.Replace("\r", string.Empty);
            body = body.Replace("\t", string.Empty);
            body = body.Replace(" {", "{");
            body = body.Replace(" :", ":");
            body = body.Replace(": ", ":");
            body = body.Replace(", ", ",");
            body = body.Replace("; ", ";");
            body = body.Replace(";}", "}");
            body = Regex.Replace(body, @"/\*(.|\n)*?\*/", string.Empty);
            //body = Regex.Replace(body, @"(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,}(?=&nbsp;)|(?<=&nbsp;)\s{2,}(?=[<])", string.Empty);

            return body;
        }
    }
}
