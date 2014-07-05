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
    /// Base class for all of the Visual SourceSafe tasks.
    /// </summary>
    public abstract class VssBase : Task
    {
        private VSSDatabase _database;
        private IVSSItem _item;
        private string _databasePath;
        private string _path;
        private string _password = string.Empty;
        private string _userName = string.Empty;
        private string _version;

        /// <summary>
        /// The path to the folder that contains the srcsafe.ini file.
        /// </summary>
        [Required]
        public string DatabasePath
        {
            get { return _databasePath; }
            set { _databasePath = value; }
        }

        /// <summary>
        /// The Visual SourceSafe project or file to perform the action
        /// on (starts with "$/").
        /// </summary>
        [Required]
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        /// <summary>
        /// The name of the user accessing the SourceSafe database.
        /// </summary>
        [Required]
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        /// <summary>
        /// A version of the <see cref="Path"/> to reference.
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// The password to use to log in to SourceSafe.
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// Represents the VSS Database
        /// </summary>
        protected VSSDatabase Database
        {
            get { return _database; }
            set { _database = value; }
        }

        /// <summary>
        /// Represents the VSS item selected (file or project).
        /// </summary>
        protected IVSSItem Item
        {
            get { return _item; }
            set { _item = value; }
        }

        /// <summary>
        /// Attempts to connect to the SourceSafe Database and
        /// load the specified item, or version of the item, if specified.
        /// </summary>
        protected void ConnectToDatabase()
        {
            _database = new VSSDatabase();
            _database.Open(new FileInfo(DatabasePath).FullName, UserName, Password);

            _item = _database.get_VSSItem(Path, false);
            if (Version != null)
            {
                _item = _item.get_Version(Version);
            }
        }
        
        /// <summary>
        /// Reserved.
        /// </summary>
        /// <returns>Reserved.</returns>
        public override bool Execute()
        {
            throw new InvalidOperationException("You cannot execute this task directly.");
        }

        /// <summary>
        /// Logs an exception using the MSBuild logging framework.
        /// </summary>
        /// <param name="e">The <see cref="Exception"/> to log.</param>
        protected void LogErrorFromException(Exception e)
        {
#if DEBUG
            Log.LogErrorFromException(e, true);
#else
            Log.LogErrorFromException(e);
#endif
        }
    }
}
