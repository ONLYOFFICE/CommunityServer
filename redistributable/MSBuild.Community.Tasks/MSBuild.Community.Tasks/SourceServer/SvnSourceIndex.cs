#region Copyright © 2009 MSBuild Community Task Project. All rights reserved.
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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MSBuild.Community.Tasks.Subversion;



namespace MSBuild.Community.Tasks.SourceServer
{
    /// <summary>
    /// A subversion source index task.
    /// </summary>
    public class SvnSourceIndex : SourceIndexBase
    {
        private XmlSerializer _infoSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SvnSourceIndex"/> class.
        /// </summary>
        public SvnSourceIndex()
        {
            SourceCommandFormat = "svn.exe export %var2%%var3%@%var4% %srcsrvtrg% --non-interactive --trust-server-cert --quiet";
        }


        /// <summary>
        /// Gets or sets the name of the source server.
        /// </summary>
        /// <value>The name of the source server.</value>
        [Required]
        public string SourceServerName { get; set; }

        /// <summary>
        /// Adds the source properties to the symbol file.
        /// </summary>
        /// <param name="symbolFile">The symbol file to add the source properties to.</param>
        /// <returns>
        /// 	<c>true</c> if successful; otherwise <c>false</c>.
        /// </returns>
        protected override bool AddSourceProperties(SymbolFile symbolFile)
        {
            IList<SourceFile> sourceFiles = symbolFile.SourceFiles;

            string output;
            if (!GetSourceInfomation(sourceFiles, out output))
                return false;

            Info info;
            if (!ConvertOutput(output, out info))
                return false;

            foreach (var sourceFile in sourceFiles)
            {
                if (!info.Entries.Contains(sourceFile.File.FullName))
                {
                    Log.LogWarning("Information for source file '{0}' was not found.", sourceFile.File.FullName);
                    continue;
                }

                var entry = info.Entries[sourceFile.File.FullName];
                AddProperties(sourceFile, entry);
            }

            return true;
        }

        private bool ConvertOutput(string output, out Info info)
        {
            info = null;
            if (string.IsNullOrEmpty(output))
            {
                Failed++;
                Log.LogError("Error parsing svn info xml information, output is null.");
                return false;
            }

            // reuse for multiple calls. no need to make static, lifetime of msbuild.exe is short.
            if (_infoSerializer == null)
                _infoSerializer = new XmlSerializer(typeof(Info));

            try
            {
                using (var sr = new StringReader(output))
                using (var reader = XmlReader.Create(sr))
                {
                    info = _infoSerializer.Deserialize(reader) as Info;
                }
            }
            catch (Exception ex)
            {
                Failed++;
                Log.LogErrorFromException(ex);
                return false;
            }

            if (info == null)
            {
                Failed++;
                Log.LogError("Error parsing svn info xml information, info object is null.");
                return false;
            }

            return true;
        }

        private bool GetSourceInfomation(IList<SourceFile> sourceFiles, out string output)
        {
            output = string.Empty;

            // get all the svn info in one call
            SvnClient client = new SvnClient();
            CopyBuildEngine(client);

            // using target file to prevent hitting max command line lenght
            string targetFile = Path.GetTempFileName();

            try
            {
                // dumping source file list to target file
                using (var sw = File.CreateText(targetFile))
                    foreach (var sourceFile in sourceFiles)
                        // can only get info on files that exists
                        if (File.Exists(sourceFile)) 
                            sw.WriteLine(sourceFile);

                client.Command = "info";
                client.TargetFile = targetFile;
                client.Xml = true;

                client.Execute();
            }
            finally
            {
                File.Delete(targetFile);
            }

            output = client.StandardOutput;
            return true;
        }

        private void AddProperties(SourceFile sourceFile, Entry entry)
        {
            sourceFile.Properties["Revision"] = entry.Revision;

            string baseUri = entry.Repository.Root;
            if (!baseUri.EndsWith("/"))
                baseUri += "/";

            sourceFile.Properties["Root"] = baseUri;
            sourceFile.Properties["Url"] = entry.Url;

            Uri root = new Uri(baseUri);
            Uri fullPath = new Uri(entry.Url);
            Uri itemPath = root.MakeRelativeUri(fullPath);

            sourceFile.Properties["ItemPath"] = itemPath.ToString();

            sourceFile.Properties["CommitRevision"] = entry.Commit.Revision;
            sourceFile.Properties["CommitAuthor"] = entry.Commit.Author;

            if (entry.Commit.DateSpecified)
                sourceFile.Properties["CommitDate"] = entry.Commit.Date;

            sourceFile.Properties["Kind"] = entry.Kind;

            if (!string.Equals(entry.WorkingCopy.Schedule, "normal", StringComparison.OrdinalIgnoreCase))
                Log.LogWarning("Source file '{0}' has pending changes. Index may point to incorrect revision.", sourceFile.File.FullName);
            else if (entry.Revision == 0)
                Log.LogWarning("Source file '{0}' has a revision of zero.", sourceFile.File.FullName);

            sourceFile.IsResolved = true;
        }

        /// <summary>
        /// Creates the source index file.
        /// </summary>
        /// <param name="symbolFile">The symbol file to create the index file from.</param>
        /// <param name="sourceIndexFile">The source index file.</param>
        /// <returns>
        /// 	<c>true</c> if successful; otherwise <c>false</c>.
        /// </returns>
        protected override bool CreateSourceIndexFile(SymbolFile symbolFile, string sourceIndexFile)
        {
            using (var writer = File.CreateText(sourceIndexFile))
            {
                writer.WriteLine("SRCSRV: ini ------------------------------------------------");
                writer.WriteLine("VERSION=1");
                writer.WriteLine("INDEXVERSION=2");
                writer.WriteLine("VERCTRL=Subversion");
                writer.WriteLine("DATETIME={0}", DateTime.UtcNow.ToString("u"));
                writer.WriteLine("SRCSRV: variables ------------------------------------------");

                // clean name
                string name = string.IsNullOrEmpty(SourceServerName) ? "Name" : SourceServerName.Trim();
                name = Regex.Replace(name, "\\W", "");

                string target = string.IsNullOrEmpty(SourceTargetFormat)
                    ? string.Format("%targ%\\{0}\\%fnbksl%(%var3%)\\%var4%\\%fnfile%(%var1%)", name)
                    : SourceTargetFormat;

                name = name.ToUpperInvariant();

                writer.WriteLine("SVN_{0}_AUTH=", name);
                writer.WriteLine("SVN_{0}_TRG={1}", name, target);
                writer.WriteLine("SVN_{0}_CMD={1} %SVN_{0}_AUTH%", name, SourceCommandFormat);

                writer.WriteLine("SRCSRVTRG=%SVN_{0}_TRG%", name);
                writer.WriteLine("SRCSRVCMD=%SVN_{0}_CMD%", name);

                writer.WriteLine("SRCSRV: source files ---------------------------------------");

                foreach (var file in symbolFile.SourceFiles)
                    if (file.IsResolved)
                        writer.WriteLine(file.ToSourceString("{File}*{Root}*{ItemPath}*{Revision}"));

                writer.WriteLine("SRCSRV: end ------------------------------------------------");

                writer.Flush();
                writer.Close();
            }

            return true;
        }
    }
}
