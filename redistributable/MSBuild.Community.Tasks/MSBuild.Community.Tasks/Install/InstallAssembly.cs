

using System;
using Microsoft.Build.Utilities;
using System.Collections;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks.Install
{
    // NOTE: I use the href + MSDN2 URL for the references in the remarks below because 
    // the System.Configuration.Install assembly is not referenced by this project, 
    // so a standard cref will not resolve correctly.

    /// <summary>
    /// Installs assemblies.
    /// </summary>
    ///<remarks>
    /// Uses InstallUtil.exe to execute the 
    /// <see href="http://msdn2.microsoft.com/system.configuration.install.installer.install.aspx">Install</see>
    /// method of
    /// <see href="http://msdn2.microsoft.com/system.configuration.install.installer.aspx">Installer</see>
    /// classes contained within specified assemblies.
    /// </remarks>
    /// <include file='..\AdditionalDocumentation.xml' path='docs/task[@name="InstallAssembly"]/*'/>
    public class InstallAssembly : ToolTask
    {
        private Hashtable bag = new Hashtable();
        
        /// <summary>
        /// The assemblies to process, identified by their assembly name.
        /// </summary>
        public ITaskItem[] AssemblyNames
        {
            get { return bag["AssemblyNames"] as ITaskItem[]; }
            set { bag["AssemblyNames"] = value; }
        }

        /// <summary>
        /// The assemblies to process, identified by their filename.
        /// </summary>
        public ITaskItem[] AssemblyFiles
        {
            get { return bag["AssemblyFiles"] as ITaskItem[]; }
            set { bag["AssemblyFiles"] = value; }
        }

        /// <summary>
        /// The file to write installation progress to.
        /// </summary>
        /// <remarks>Set to a single space to disable logging to a file.
        /// <para>
        /// If not specified, the default is to log to [assemblyname].installLog
        /// </para>
        /// </remarks>
        public string LogFile
        {
            get { return bag["LogFile"] as string; }
            set { bag["LogFile"] = value; }
        }

        bool showCallStack = false;

        /// <summary>
        ///  If an exception occurs at any point during installation, the call
        ///  stack will be printed to the log.
        ///  </summary>
        public bool ShowCallStack
        {
            get { return showCallStack; }
            set { showCallStack = value; }
        }

        /// <summary>
        /// Determines whether assemblies are installed or uninstalled.
        /// </summary>
        protected virtual bool IsUninstall { get { return false; } }

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            if (String.IsNullOrEmpty(ToolPath))
            {
                // provide a reasonable default
                return ToolLocationHelper.GetPathToDotNetFrameworkFile(ToolName, TargetDotNetFrameworkVersion.VersionLatest);
            }

            return System.IO.Path.Combine(ToolPath, ToolName);
        }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the executable file to run.</returns>
        protected override string ToolName
        {
            get { return "InstallUtil.exe"; }
        }

        /// <summary>
        /// Returns a string value containing the command line arguments 
        /// to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass 
        /// directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder commandLine = new CommandLineBuilder();
            if (ShowCallStack)
            {
                commandLine.AppendSwitch("/ShowCallStack");
            }
            if (LogFile != null && LogFile.Trim().Length == 0)
            {
                commandLine.AppendSwitch("/LogFile=");
            }
            else
            {
                commandLine.AppendSwitchIfNotNull("/LogFile=", LogFile);
            }

            if (IsUninstall)
            {
                commandLine.AppendSwitch("/uninstall");
            }
            commandLine.AppendFileNamesIfNotNull(AssemblyFiles, " ");
            if (AssemblyNames != null)
            {
                foreach (ITaskItem assemblyName in AssemblyNames)
                {
                    commandLine.AppendSwitch("/AssemblyName");
                    commandLine.AppendFileNameIfNotNull(assemblyName);
                }
            }
            return commandLine.ToString();
        }
    }
}
