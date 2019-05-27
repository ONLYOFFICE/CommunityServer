using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MSBuild.Community.Tasks.Tfs;

namespace MSBuild.Community.Tasks.SourceServer
{
    /// <summary>
    /// Task to index pdb files and entries to retrieve source files from Team Foundation Server source control.
    /// </summary>
    /// <remarks>
    /// This implementation is based on a pdb indexed by Team Foundation Build 2013 and has not been tested on other versions
    /// of TFS.
    /// </remarks>
    /// <example>Index a PDB.
    /// <code><![CDATA[
    /// <TfsSourceIndex SymbolFiles="@(Symbols)" TeamProjectCollectionUri="http://my-tfsserver/tfs/DefaultCollection" />
    /// ]]></code>
    /// </example>
    public class TfsSourceIndex : SourceIndexBase
    {
        /// <summary>
        /// Path to the root of the team collection hosting your project 
        /// </summary>
        /// <example>http://my-tfsserver/tfs/DefaultCollection</example>
        [Required]
        public string TeamProjectCollectionUri { get; set; }

        /// <summary>
        /// Gets or sets the root of the workspace where the <see cref="SourceIndexBase.SymbolFiles"/>.
        /// </summary>
        public string WorkspaceDirectory { get; set; }

        /// <summary>
        /// Gets or sets the tfs collection uri to use. 
        /// When this options is set pdb source information is retrieved from the server, rather
        /// that using local workspace information.
        /// </summary>
        /// <value>
        /// The collection.
        /// </value>
        /// <remarks>
        /// Also set the <see cref="ChangesetVersion"/> to retrieve the correct changeset
        /// </remarks>
        /// <example>http://mytfs.com:8080/tfs/DefaultCollection</example>
        public string Collection { get; set; }

        /// <summary>
        /// Gets or sets the change set used to retrieve file information.
        /// </summary>
        /// <value>
        /// The change set number.
        /// </value>
        public string ChangesetVersion { get; set; }

        [Required]
        public string TeamProjectRootDirectory { get; set; }
        
        [Required]
        public string TeamProjectName { get; set; }

        /// <inheritdoc />
        public override bool Execute()
        {
            return base.Execute() && this.Failed == 0;
        }

        /// <inheritdoc />
        protected override bool AddSourceProperties(SymbolFile symbolFile)
        {
            var itemInformation = GetItemInformation(symbolFile.SourceFiles.Select(item => new TaskItem(item.File.FullName)).Cast<ITaskItem>());
            foreach (var item in symbolFile.SourceFiles)
            {
                var key = GetItemSpec(item.File.FullName, true).ToLower();
                if (!itemInformation.ContainsKey(key))
                {
                    var allKeys = new StringBuilder();
                    foreach (var entry in itemInformation)
                    {
                        allKeys.Append(entry.Key + "|");
                    }

                    throw new KeyNotFoundException(string.Format("Could not find a local information entry for |{0}|.\n{1}.", key, allKeys));
                }

                item.Properties["FileName"] = item.File.Name;
                item.Properties["Revision"] = itemInformation[key].Changeset;
                item.Properties["ItemPath"] = itemInformation[key].ServerPath.TrimStart('$');
                item.IsResolved = true;
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool CreateSourceIndexFile(SymbolFile symbolFile, string sourceIndexFile)
        {
            using (var writer = File.CreateText(sourceIndexFile))
            {
                writer.WriteLine("SRCSRV: ini ------------------------------------------------");
                writer.WriteLine("VERSION=3");
                writer.WriteLine("INDEXVERSION=2");
                writer.WriteLine("VERCTRL=Team Foundation Server");
                writer.WriteLine("DATETIME={0}", DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss yyyy")); // strange format used by TFS, just copied in case its used
                writer.WriteLine("INDEXER=MSCT"); // MSBUILD Community tasks
                writer.WriteLine("SRCSRV: variables ------------------------------------------");
                writer.WriteLine(@"TFS_EXTRACT_CMD=tf.exe view /version:%var4% /noprompt ""$%var3%"" /server:%fnvar%(%var2%) /console >%srcsrvtrg%");
                writer.WriteLine(@"TFS_EXTRACT_TARGET=%targ%\%var2%%fnbksl%(%var3%)\%var4%\%fnfile%(%var5%)");
                writer.WriteLine("SRCSRVVERCTRL=tfs");
                writer.WriteLine("SRCSRVERRDESC=access");
                writer.WriteLine("SRCSRVERRVAR=var2");
                writer.WriteLine(string.Format("VSTFSSERVER={0}", this.TeamProjectCollectionUri));
                writer.WriteLine("SRCSRVTRG=%TFS_extract_target%");
                writer.WriteLine("SRCSRVCMD=%TFS_extract_cmd%");
                writer.WriteLine("SRCSRV: source files ---------------------------------------");

                foreach (var file in symbolFile.SourceFiles)
                    if (file.IsResolved)
                        writer.WriteLine(file.ToSourceString("{File}*VSTFSSERVER*{ItemPath}*{Revision}*{FileName}"));

                writer.WriteLine("SRCSRV: end ------------------------------------------------");

                writer.Flush();
                writer.Close();
            }

            return true;
        }

        public bool ServerMode
        {
            get
            {
                return !string.IsNullOrEmpty(this.ChangesetVersion);
            }
        }

        private Dictionary<string, IItemInformation> GetItemInformation(IEnumerable<ITaskItem> files)
        {                        
            var client = new TfsClient();
            CopyBuildEngine(client);
            
            var success = false;
            var attempt = 0; const int retryCount = 3;
            
            while (!success && attempt < retryCount)
            {
                Log.LogMessage("Retrieving local file information to determine tfs source indexing attempt {0}", attempt + 1);           
                client.Command = "info";
                if (!string.IsNullOrEmpty(this.ChangesetVersion))
                {
                    client.ChangesetVersion = this.ChangesetVersion;
                }

                if (!string.IsNullOrEmpty(this.TeamProjectCollectionUri))
                {
                    client.Collection = this.TeamProjectCollectionUri;
                }

                client.Files = GetItemSpecs(files);
                client.WorkingDirectory = this.WorkspaceDirectory;
                success = client.Execute();
                attempt = attempt + 1;
            }

            Log.LogMessage("Success {0}, exit code {1} after {2} attempts.", success.ToString().ToLower(), client.ExitCode, attempt);

            
            var infoCommandResponse = new InfoCommandResponse(client.Output.ToString());
            return this.ServerMode  ? infoCommandResponse.ServerInformation : infoCommandResponse.LocalInformation;
        }

        private ITaskItem[] GetItemSpecs(IEnumerable<ITaskItem> files)
        {          
            return files.ToArray()
                .Select(item => new TaskItem(GetItemSpec(item.ItemSpec, this.ServerMode)))
                .Cast<ITaskItem>()
                .ToArray();
        }

        private string GetItemSpec(string value, bool serverMode)
        {
            if (serverMode)
            {
                return 
                    string.Format("$/{0}{1}", 
                        this.TeamProjectName, 
                        ReplaceString(value, this.TeamProjectRootDirectory, string.Empty, StringComparison.CurrentCultureIgnoreCase).Replace('\\', '/'));
            }

            if (!string.IsNullOrEmpty(this.WorkspaceDirectory))
            {
                return 
                    ReplaceString(value, this.WorkspaceDirectory, string.Empty, StringComparison.InvariantCultureIgnoreCase)
                    .TrimStart('\\');    
            }

            return value;
        }

        public static string ReplaceString(string value, string oldValue, string newValue, StringComparison comparison)
        {
            var result = new StringBuilder();

            int previousIndex = 0;
            int index = value.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                result.Append(value.Substring(previousIndex, index - previousIndex));
                result.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = value.IndexOf(oldValue, index, comparison);
            }
            result.Append(value.Substring(previousIndex));

            return result.ToString();
        }
    }
}