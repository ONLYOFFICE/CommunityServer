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

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;



namespace MSBuild.Community.Tasks.Subversion
{
    ///<summary>
    /// Subversion status command.
    ///</summary>
    public class SvnStatus : SvnClient
    {
        ///<summary>
        /// Creates an instance of SvnStatus.
        ///</summary>
        public SvnStatus()
        {
            base.Command = "status";
            base.NonInteractive = true;
            base.NoAuthCache = true;
            base.Xml = true;
        }

        /// <summary>
        /// Runs the exectuable file with the specified task parameters.
        /// </summary>
        /// <returns>
        /// true if the task runs successfully; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            bool result = base.Execute();
            if (result)
            {
                Parse();
            }
            return result;
        }

        private void Parse()
        {
            using (var sr = new StringReader(StandardOutput))
            using (var reader = XmlReader.Create(sr))
            {
                TaskItem currentItem = null;
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    var name = reader.Name;

                    switch (name)
                    {
                        case "entry":
                            string file = reader.GetAttribute("path");
                            currentItem = new TaskItem(file);
                            _modified.Add(currentItem);
                            break;

                        case "wc-status":
                            string props = reader.GetAttribute("props");
                            string item = reader.GetAttribute("item");
                            string revision = reader.GetAttribute("revision");

                            if (currentItem != null)
                            {
                                if (props != null) currentItem.SetMetadata("Props", props);
                                if (item != null) currentItem.SetMetadata("Item", item);
                                if (revision != null) currentItem.SetMetadata("Revision", revision);
                            }
                            break;

                        case "commit":
                            break;

                        case "author":
                            var author = reader.Value;

                            if (currentItem != null)
                            {
                                if (author != null) currentItem.SetMetadata("Author", author);
                            }
                            break;

                        case "date":
                            var date = reader.Value;

                            if (currentItem != null)
                            {
                                if (date != null) currentItem.SetMetadata("Date", date);
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        [Output]
        public ITaskItem[] Entries
        {
            get { return _modified.ToArray(); }
            set { _modified = new List<ITaskItem>(value); }
        }

        private List<ITaskItem> _modified = new List<ITaskItem>();
    }
}