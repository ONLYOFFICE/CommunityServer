#region Copyright © 2008 MSBuild Community Task Project. All rights reserved.

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

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MSBuild.Community.Tasks.Properties;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Unzip a file to a target directory.
    /// </summary>
    /// <example>Unzip file tasks
    /// <code><![CDATA[
    /// <Unzip ZipFileName="MSBuild.Community.Tasks.zip" 
    ///     TargetDirectory="Backup"/>
    /// ]]></code>
    /// </example>
    public class Unzip : Task
    {
        private readonly List<ITaskItem> _files = new List<ITaskItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Unzip"/> class.
        /// </summary>
        public Unzip()
        {
            Overwrite = true;
        }

        /// <summary>
        /// Gets or sets the name of the zip file.
        /// </summary>
        /// <value>The name of the zip file.</value>
        [Required]
        public string ZipFileName { get; set; }

        /// <summary>
        /// Gets or sets the target directory.
        /// </summary>
        /// <value>The target directory.</value>
        [Required]
        public string TargetDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite any existing files on extraction. Defaults to <c>true</c>.
        /// </summary>
        /// <value><c>true</c> to overwrite any existing files on extraction; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to output less information. Defaults to <c>false</c>.
        /// </summary>
        /// <value><c>false</c> to output a message for every file extracted; otherwise, <c>true</c>.</value>
        [DefaultValue(false)]
        public bool Quiet { get; set; }

        /// <summary>
        /// Gets the files extracted from the zip.
        /// </summary>
        /// <value>The files extracted from the zip.</value>
        [Output]
        public ITaskItem[] ExtractedFiles
        {
            get { return _files.ToArray(); }
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            if (!File.Exists(ZipFileName))
            {
                Log.LogError(Resources.ZipFileNotFound, ZipFileName);
                return false;
            }

            if (!Directory.Exists(TargetDirectory))
                Directory.CreateDirectory(TargetDirectory);

            Log.LogMessage(Resources.UnzipFileToDirectory, ZipFileName, TargetDirectory);

            ExtractExistingFileAction acting = Overwrite 
                ? ExtractExistingFileAction.OverwriteSilently 
                : ExtractExistingFileAction.DoNotOverwrite;

            using (var zip = new ZipFile(ZipFileName))
            {
                zip.ExtractProgress += OnExtractProgress;
                zip.ExtractAll(TargetDirectory, acting);
            }

            Log.LogMessage(Resources.UnzipSuccessfully, ZipFileName);
            return true;
        }

        private void OnExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e == null || e.CurrentEntry == null)
                return;

            if (_files.All(f => f.ItemSpec != e.CurrentEntry.FileName)) {
                _files.Add(new TaskItem(e.CurrentEntry.FileName));
                if (!Quiet) {
                    Log.LogMessage(Resources.UnzipExtracted, e.CurrentEntry.FileName);
                }
            }
        }
    }
}