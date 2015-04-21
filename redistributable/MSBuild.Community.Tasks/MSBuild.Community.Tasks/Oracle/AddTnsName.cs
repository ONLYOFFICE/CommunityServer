
using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;
using MSBuild.Community.Tasks.Services;

namespace MSBuild.Community.Tasks.Oracle
{
    /// <summary>
    /// Defines a database host within the Oracle TNSNAMES.ORA file.
    /// </summary>
    /// <example>Add an entry to the system default TNSNAMES.ORA file and update any entry that already exists:
    /// <code><![CDATA[ <AddTnsName AllowUpdates="True" EntryName="northwind.world" EntryText="(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = northwinddb01)(PORT = 1521))) (CONNECT_DATA = (SERVICE_NAME = northwind.world)))"  /> ]]>
    /// </code>
    /// </example>
    /// <example>Add an entry to a specific file and fail if the entry already exists:
    /// <code><![CDATA[ <AddTnsName TnsNamesFile="c:\oracle\network\admin\tnsnames.ora" AllowUpdates="False" EntryName="northwind.world" EntryText="(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = northwinddb01)(PORT = 1521))) (CONNECT_DATA = (SERVICE_NAME = northwind.world)))"  /> ]]>
    /// </code>
    /// </example>
    public class AddTnsName : Task
    {
        private IRegistry registry;
        private IFilesSystem fileSystem;

        /// <summary>
        /// Creates a new instance of the AddTnsName task using dependency injection.
        /// </summary>
        /// <param name="registry">A service that provides access to the Windows registry.</param>
        /// <param name="fileSystem">A service that provides access to the file system.</param>
        public AddTnsName(IRegistry registry, IFilesSystem fileSystem)
        {
            this.registry = registry;
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Creates a new instance of the AddTnsName task using the default system services.
        /// </summary>
        public AddTnsName() : this(new Win32Registry(), new FileSystem()) { }

        ///<summary>
        ///When overridden in a derived class, executes the task.
        ///</summary>
        ///
        ///<returns>
        ///true if the task successfully executed; otherwise, false.
        ///</returns>
        public override bool Execute()
        {
            modifiedFile = GetEffectivePathToTnsNamesFile();
            if (String.IsNullOrEmpty(ModifiedFile))
            {
                Log.LogError(Properties.Resources.TnsNamesFileNotFound);
                return false;
            }
            originalFileText = fileSystem.ReadTextFromFile(modifiedFile);

            TnsParser parser = new TnsParser(originalFileText);
            TnsEntry targetEntry = parser.FindEntry(entryName);
            if (targetEntry == null)
            {
                // append entry definition to the beginning of the file
                modifiedFileText = String.Format("{0} = {1}{2}{3}", entryName, entryText, Environment.NewLine, originalFileText);
                Log.LogMessage(Properties.Resources.TnsnameAdded, entryName, modifiedFile);
            }
            else
            {
                if (!allowUpdates)
                {
                    Log.LogError(Properties.Resources.TnsnameUpdateAborted, entryName, modifiedFile);
                    return false;
                }
                // replace the entry definition within the file
                string beforeEntry = originalFileText.Substring(0, targetEntry.StartPosition);
                string afterEntry = originalFileText.Substring(targetEntry.StartPosition + targetEntry.Length);
                modifiedFileText = String.Format("{0}{1} = {2}{3}", beforeEntry, entryName, entryText, afterEntry);
                Log.LogMessage(Properties.Resources.TnsnameUpdated, entryName, modifiedFile);
            }
            fileSystem.WriteTextToFile(modifiedFile, modifiedFileText);
            return true;
        }

        /// <summary>
        /// The path to a specific TNSNAMES.ORA file to update.
        /// </summary>
        /// <remarks>If not specified, the default is %ORACLE_HOME%\network\admin\tnsnames.ora</remarks>
        public string TnsNamesFile
        {
            set { tnsNamesFile = value; }
        }

        /// <summary>
        /// The contents of the TNSNAMES.ORA file before any changes are made.
        /// </summary>
        [Output]
        public string OriginalFileText
        {
            get { return originalFileText; }
        }

        /// <summary>
        /// The path to the TNSNAMES.ORA that was used by task.
        /// </summary>
        [Output]
        public string ModifiedFile
        {
            get { return modifiedFile; }
        }

        /// <summary>
        /// The name of the host entry to add.
        /// </summary>
        /// <remarks>To be properly recognized by Oracle, the value must contain a period, followed by a suffix. For example: mydatabase.world</remarks>
        [Required]
        public string EntryName
        {
            set { entryName = value; }
        }

        /// <summary>
        /// The contents of the TNSNAMES.ORA file after the task executes.
        /// </summary>
        [Output]
        public string ModifiedFileText
        {
            get { return modifiedFileText; }
        }

        /// <summary>
        /// The definition of the host entry to add.
        /// </summary>
        /// <remarks>To be properly recognized by Oracle, the value must be surrounded by parentheses.</remarks>
        public string EntryText
        {
            set { entryText = value; }
        }

        /// <summary>
        /// When true, the task will update an existing entry with <see cref="EntryName"/>. 
        /// If false, the task will fail if <see cref="EntryName"/> already exists.
        /// </summary>
        public bool AllowUpdates
        {
            set { allowUpdates = value; }
        }

        /// <summary>
        /// Determines which TNSNAMES.ORA file to update based on task input and the current system environment.
        /// </summary>
        /// <returns>The path of the TNSNAMES.ORA file that will be used by the task.</returns>
        /// <exclude />
        public string GetEffectivePathToTnsNamesFile()
        {
            if (!String.IsNullOrEmpty(tnsNamesFile))
            {
                return tnsNamesFile;
            }
            string[] oracleHomes = registry.GetSubKeys(RegistryHive.LocalMachine, ORACLE_REGISTRY);
            foreach (string home in oracleHomes)
            {
                string homePathKey = String.Format(@"HKEY_LOCAL_MACHINE\{0}\{1}", ORACLE_REGISTRY, home);
                Log.LogMessage(MessageImportance.Low, Properties.Resources.OracleHomeCheck, homePathKey);
                string homePath = registry.GetValue(homePathKey, ORACLE_HOME) as string;
                if (homePath == null) continue;
                string tnsNamesPath = Path.Combine(homePath, TNSNAMES_PATH);
                Log.LogMessage(MessageImportance.Low, Properties.Resources.TnsNamesFileCheck, tnsNamesPath);
                if (fileSystem.FileExists(tnsNamesPath))
                {
                    return tnsNamesPath;
                }
            }
            return null;
        }
        private const string ORACLE_REGISTRY = @"SOFTWARE\ORACLE";
        private const string ORACLE_HOME = "ORACLE_HOME";
        private const string TNSNAMES_PATH = @"NETWORK\ADMIN\tnsnames.ora";

        private string tnsNamesFile;
        private string originalFileText;
        private string modifiedFile;
        private string entryName;
        private string modifiedFileText;
        private string entryText;
        private bool allowUpdates;
    }
}
