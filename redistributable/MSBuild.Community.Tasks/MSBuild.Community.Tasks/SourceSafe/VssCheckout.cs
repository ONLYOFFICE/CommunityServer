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
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.SourceSafe.Interop;



namespace MSBuild.Community.Tasks.SourceSafe
{
    /// <summary>
    /// Task that executes a checkout of files or projects
    /// against a Visual SourceSafe database.
    /// </summary>
    /// <example>
    /// <para></para>
    /// <code><![CDATA[<VssCheckout UserName="ccnet_build"
    ///   Password="build"
    ///   LocalPath="C:\Dev\MyProjects\Project"
    ///   Recursive="False"
    ///   DatabasePath="\\VSSServer\VSS\srcsafe.ini"
    ///   Path="$/Test/TestFile.cs"
    /// />
    /// ]]></code>
    /// </example>
    public class VssCheckout : VssRecursiveBase
    {
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
        /// Determines whether files will be writable once retrieved from
        /// the repository. The default is <see langword="false"/>.
        /// </summary>
        public bool Writable
        {
            get { return _writable; }
            set { _writable = value; }
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
                string searchPattern = "";
                if (Path.Contains("*"))
                {
                    // If the path has a wildcard character (*), we use the last part (or the search pattern part) 
                    // of the path to selectively checkout files based on wildcard search using regular expression. 
                    // In order to achieve this we strip out the search pattern part of the path, if the path has 
                    // a wildcard character in it, and use it for the pattern in our regular expression search, but 
                    // only if search pattern part contains a wildcard character.
                    // Example: $/Project/AssemblyInfo.*
                    Path = Path.TrimEnd(new char[] { '/' });
                    searchPattern = Path.Substring(Path.LastIndexOf('/') + 1);
                    searchPattern = searchPattern.Replace(".", "\\.").Replace("*", ".*");
                    if (searchPattern.Contains("*"))
                    {
                        // The search pattern part of the path is valid, so we'll remove the search pattern from
                        // the path. This way our starting point will be a project and not a file.
                        Path = Path.Substring(0, Path.LastIndexOf('/'));
                    }
                    else
                    {
                        // The wildcard is not in the search pattern part of the path but elsewhere, so we'll
                        // try to checkout the provided path as-is. 
                        searchPattern = "";
                    }
                }

                ConnectToDatabase();

                int flags = (Recursive ? Convert.ToInt32(RecursiveFlag) : 0) |
                    (Writable ? Convert.ToInt32(VSSFlags.VSSFLAG_USERRONO) : Convert.ToInt32(VSSFlags.VSSFLAG_USERROYES));

                Checkout(Item, new FileInfo(_localPath), flags, searchPattern);

                Log.LogMessage(MessageImportance.Normal, "Checked out '{0}'.", Path);
                return true;
            }
            catch (Exception e)
            {
                LogErrorFromException(e);
                return false;
            }
        }

        private void Checkout(IVSSItem checkoutItem, FileInfo localPath, int flags, string pattern)
        {
            //TODO: timestamp stuff

            switch (checkoutItem.Type)
            {
                case (int)VSSItemType.VSSITEM_PROJECT:
                    // Item is a project.
                    if (string.IsNullOrEmpty(pattern))
                    {
                        // In the absence of a pattern, we'll checkout the entire project.
                        checkoutItem.Checkout("", localPath.FullName, flags);
                    }
                    else
                    {
                        // If a pattern is provided, we process all subitems in the project.
                        foreach (IVSSItem subItem in checkoutItem.get_Items(false))
                        {
                            switch (subItem.Type)
                            {
                                case (int)VSSItemType.VSSITEM_PROJECT:
                                    // Subitem is a project.
                                    if (Recursive)
                                    {
                                        // We'll recursively checkout matching files in this project.
                                        Checkout(subItem, new FileInfo(System.IO.Path.Combine(localPath.FullName, subItem.Name)), flags, pattern);
                                    }
                                    break;
                                case (int)VSSItemType.VSSITEM_FILE:
                                    // Subitem is a file.
                                    if (Regex.IsMatch(subItem.Name, pattern))
                                    {
                                        // We'll checkout this file since it matches the search pattern.
                                        Checkout(subItem, localPath, flags, "");
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case (int)VSSItemType.VSSITEM_FILE:
                    // Item is a file.
                    string filePath = System.IO.Path.Combine(localPath.FullName, checkoutItem.Name);
                    checkoutItem.Checkout("", filePath, flags);
                    break;
            }
        }
    }
}
