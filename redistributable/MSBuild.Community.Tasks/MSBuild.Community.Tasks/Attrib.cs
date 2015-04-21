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
 * 
 * James Higgs (james.higgs@interesource.com)
*/
#endregion



using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Changes the attributes of files and/or directories
    /// </summary>
    /// <example>
    /// <para>Make file Readonly, Hidden and System.</para>
    /// <code><![CDATA[
    /// <Attrib Files="Test\version.txt" 
    ///     ReadOnly="true" Hidden="true" System="true"/>
    /// ]]></code>
    /// <para>Clear Hidden and System attributes.</para>
    /// <code><![CDATA[
    /// <Attrib Files="Test\version.txt" 
    ///     Hidden="false" System="false"/>
    /// ]]></code>
    /// <para>Make file Normal.</para>
    /// <code><![CDATA[
    /// <Attrib Files="Test\version.txt" 
    ///     Normal="true"/>
    /// ]]></code>
    /// </example>
    public class Attrib : Task
    {
        #region Properties
        private Dictionary<string, bool> attributeBag = new Dictionary<string, bool>();

        private ITaskItem[] _files;

        /// <summary>
        /// Gets or sets the list of files to change attributes on.
        /// </summary>
        /// <value>The files to change attributes on.</value>
        public ITaskItem[] Files
        {
            get { return _files; }
            set { _files = value; }
        }

        private ITaskItem[] _directories;

        /// <summary>
        /// Gets or sets the list of directories to change attributes on.
        /// </summary>
        /// <value>The directories to change attributes on.</value>
        public ITaskItem[] Directories
        {
            get { return _directories; }
            set { _directories = value; }
        }

        /// <summary>
        /// Gets or sets file's archive status.
        /// </summary>
        /// <value><c>true</c> if archive; otherwise, <c>false</c>.</value>
        public bool Archive
        {
            get { return attributeBag.ContainsKey("Archive") ? attributeBag["Archive"] : false; }
            set { attributeBag["Archive"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating file is compressed.
        /// </summary>
        /// <value><c>true</c> if compressed; otherwise, <c>false</c>.</value>
        public bool Compressed
        {
            get { return attributeBag.ContainsKey("Compressed") ? attributeBag["Compressed"] : false; }
            set { attributeBag["Compressed"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating file is encrypted.
        /// </summary>
        /// <value><c>true</c> if encrypted; otherwise, <c>false</c>.</value>
        public bool Encrypted
        {
            get { return attributeBag.ContainsKey("Encrypted") ? attributeBag["Encrypted"] : false; }
            set { attributeBag["Encrypted"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating file is hidden, and thus is not included in an ordinary directory listing.
        /// </summary>
        /// <value><c>true</c> if hidden; otherwise, <c>false</c>.</value>
        public bool Hidden
        {
            get { return attributeBag.ContainsKey("Hidden") ? attributeBag["Hidden"] : false; }
            set { attributeBag["Hidden"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating file is normal and has no other attributes set.
        /// </summary>
        /// <value><c>true</c> if normal; otherwise, <c>false</c>.</value>
        public bool Normal
        {
            get { return attributeBag.ContainsKey("Normal") ? attributeBag["Normal"] : false; }
            set { attributeBag["Normal"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating file is read-only.
        /// </summary>
        /// <value><c>true</c> if read-only; otherwise, <c>false</c>.</value>
        public bool ReadOnly
        {
            get { return attributeBag.ContainsKey("ReadOnly") ? attributeBag["ReadOnly"] : false; }
            set { attributeBag["ReadOnly"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating file is a system file.
        /// </summary>
        /// <value><c>true</c> if system; otherwise, <c>false</c>.</value>
        public bool System
        {
            get { return attributeBag.ContainsKey("System") ? attributeBag["System"] : false; }
            set { attributeBag["System"] = value; }
        } 
        #endregion

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns><see langword="true"/> if the task ran successfully; 
        /// otherwise <see langword="false"/>.</returns>
        public override bool Execute()
        {
            if (_directories != null)
            {
                foreach (ITaskItem item in _directories)
                {
                    DirectoryInfo dir = new DirectoryInfo(item.ItemSpec);
                    if (!dir.Exists)
                        continue;

                    FileAttributes flags = dir.Attributes;
                    flags = UpdateAttributes(flags);
                    Log.LogMessage(Properties.Resources.AttribDirectory, item.ItemSpec, flags.ToString());
                    dir.Attributes = flags;
                }
            }

            if (_files != null)
            {
                foreach (ITaskItem item in _files)
                {
                    FileInfo file = new FileInfo(item.ItemSpec);
                    if (!file.Exists)
                        continue;

                    FileAttributes flags = file.Attributes;
                    flags = UpdateAttributes(flags);
                    Log.LogMessage(Properties.Resources.AttribFile, item.ItemSpec, flags.ToString());
                    file.Attributes = flags;
                }
            }
            return true;
        }

        private FileAttributes UpdateAttributes(FileAttributes flags)
        {
            if (attributeBag.ContainsKey("Archive"))
            {
                if (attributeBag["Archive"])
                    flags |= FileAttributes.Archive;
                else
                    flags &= ~FileAttributes.Archive;
            }
            if (attributeBag.ContainsKey("Compressed"))
            {
                if (attributeBag["Compressed"])
                    flags |= FileAttributes.Compressed;
                else
                    flags &= ~FileAttributes.Compressed;
            }
            if (attributeBag.ContainsKey("Encrypted"))
            {
                if (attributeBag["Encrypted"])
                    flags |= FileAttributes.Encrypted;
                else
                    flags &= ~FileAttributes.Encrypted;
            }
            if (attributeBag.ContainsKey("Hidden"))
            {
                if (attributeBag["Hidden"])
                    flags |= FileAttributes.Hidden;
                else
                    flags &= ~FileAttributes.Hidden;
            }
            if (attributeBag.ContainsKey("Normal"))
            {
                if (attributeBag["Normal"])
                    flags = FileAttributes.Normal;
            }
            if (attributeBag.ContainsKey("ReadOnly"))
            {
                if (attributeBag["ReadOnly"])
                    flags |= FileAttributes.ReadOnly;
                else
                    flags &= ~FileAttributes.ReadOnly;
            }
            if (attributeBag.ContainsKey("System"))
            {
                if (attributeBag["System"])
                    flags |= FileAttributes.System;
                else
                    flags &= ~FileAttributes.System;
            }
            return flags;
        } 
    }
}
