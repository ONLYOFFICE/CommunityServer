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
    /// Task that retireves an item or project from a Visual SourceSafe database.
    /// </summary>
    /// <example>
    /// <para>Get the latest version (recursive) of a file from a VSS database and place
    /// in the specified local folder.</para>
    /// <code><![CDATA[
    /// <VssGet UserName="user"
    ///     Password="password"
    ///     LocalPath="C:\Dev\MyProjects\Project"
    ///     Recursive="True"
    ///     DatabasePath="\\VSSServer\VSS\srcsafe.ini"
    ///     Path="$/Project"
    /// ]]></code>
    /// </example>
    public class VssGet : VssRecursiveBase
    {
        private string _localPath;
        private bool _replace = false;
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
        /// Determines whether to replace writable files.
        /// The default is <see langword="false"/>.
        /// </summary>
        public bool Replace
        {
            get { return _replace; }
            set { _replace = value; }
        }

        /// <summary>
        /// Determines whether files will be writable once retrieved from
        /// the repository. The default is <see langword="false"/>.
        /// </summary>
        public bool Writable
        {
            get { return _writable; }
            set { _writable = value; }
        }
        
        /// <summary>
        /// Runs the task using the specified parameters.
        /// </summary>
        /// <returns><see langword="true"/> if the task ran successfully; 
        /// otherwise <see langword="false"/>.</returns>
        public override bool Execute()
        {
            try
            {
                FileInfo localPath = new FileInfo(_localPath);
                ConnectToDatabase();
                Log.LogMessage(MessageImportance.Normal, "Getting '{0}' to '{1}'", Path, localPath.FullName);
                
                //TODO: figure the flags out....
                int flags = (Recursive ? Convert.ToInt32(RecursiveFlag) : 0) |
                    (Writable ? Convert.ToInt32(VSSFlags.VSSFLAG_USERRONO) : Convert.ToInt32(VSSFlags.VSSFLAG_USERROYES)) |
                    (Replace ? Convert.ToInt32(VSSFlags.VSSFLAG_REPREPLACE) : 0);
                //TODO: timestamp stuff....

                string localPathValue;

                switch (Item.Type)
                {
                    case (int) VSSItemType.VSSITEM_PROJECT :
                        localPathValue = localPath.FullName;
                        Item.Get(ref localPathValue, flags);
                        break;
                    case (int) VSSItemType.VSSITEM_FILE :
                        localPathValue = System.IO.Path.Combine(localPath.FullName, Item.Name);
                        Item.Get(ref localPathValue, flags);
                        break;
                }

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
