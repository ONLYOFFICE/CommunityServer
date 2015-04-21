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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.SourceSafe.Interop;



namespace MSBuild.Community.Tasks.SourceSafe
{
    /// <summary>
    /// Task that executes a checkin against a VSS Database.
    /// </summary>
    /// <example>
    /// <para></para>
    /// <code><![CDATA[<VssCheckin UserName="ccnet_build"
    ///   Password="build"
    ///   LocalPath="C:\Dev\MyProjects\Project\TestFile.cs"
    ///   Recursive="False"
    ///   DatabasePath="\\VSSServer\VSS\srcsafe.ini"
    ///   Path="$/Test/TestFile.cs"
    /// />
    /// ]]></code>
    /// </example>
    public class VssCheckin : VssRecursiveBase
    {
        private string _comment = string.Empty;
        private string _localPath;
        private bool _writable = false;

        /// <summary>
        /// The path to the local working directory.
        /// </summary>
        [Required]
        public string LocalPath
        {
            get { return _localPath; }
            set { _localPath = value; }
        }

        /// <summary>
        /// Determines whether to leave the file(s) as writable once the
        /// checkin is complete. The default is <see langword="false"/>.
        /// </summary>
        public bool Writable
        {
            get { return _writable; }
            set { _writable = value; }
        }

        /// <summary>
        /// The checkin comment.
        /// </summary>
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns><see langword="true"/> if the task ran successfully; 
        /// otherwise <see langword="false"/>.</returns>
        public override bool Execute()
        {
            try
            {
                FileInfo localPath = new FileInfo(_localPath);
                ConnectToDatabase();

                int flags = (Recursive ? Convert.ToInt32(RecursiveFlag) : 0) |
                    (Writable ? Convert.ToInt32(VSSFlags.VSSFLAG_USERRONO) : Convert.ToInt32(VSSFlags.VSSFLAG_USERROYES));

                Item.Checkin(Comment, localPath.FullName, flags);

                Log.LogMessage(MessageImportance.Normal, "Checked in '{0}'.", Path);
                return true;
            }
            catch (Exception e)
            {
                LogErrorFromException(e);
                return false;
            }
        }
    }
}
