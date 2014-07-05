#region Copyright © 2008 MSBuild Community Task Project. All rights reserved.
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
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Reflection;
using MSBuild.Community.Tasks.Fusion;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// The list of the commands available to the GacUtil Task
    /// </summary>
    public enum GacUtilCommands
    {
        /// <summary>Install the list of assemblies into the GAC.</summary>
        Install,
        /// <summary>Uninstall the list of assembly names from the GAC.</summary>
        Uninstall,
    }

    /// <summary>
    /// MSBuild task to install and uninstall assemblies into the GAC
    /// </summary>
    /// <example>Install a dll into the GAC.
    /// <code><![CDATA[
    ///     <GacUtil 
    ///         Command="Install" 
    ///         Assemblies="MSBuild.Community.Tasks.dll" 
    ///         Force="true" />
    /// ]]></code>
    /// </example>
    /// <example>Uninstall a dll from the GAC.
    /// <code><![CDATA[
    ///     <GacUtil 
    ///         Command="Uninstall" 
    ///         Assemblies="MSBuild.Community.Tasks" 
    ///         Force="true" />
    /// ]]></code>
    /// </example>
    public class GacUtil : Task
    {

        #region Properties
        private GacUtilCommands _command = GacUtilCommands.Install;

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>The command.</value>
        /// <enum cref="MSBuild.Community.Tasks.GacUtilCommands"/>
        public string Command
        {
            get { return _command.ToString(); }
            set
            {
                if (Enum.IsDefined(typeof(GacUtilCommands), value))
                    _command = (GacUtilCommands)Enum.Parse(typeof(GacUtilCommands), value);
                else
                    throw new ArgumentException(
                        string.Format("The value '{0}' is not in the GacUtilCommans Enum.", value));
            }
        }

        private string[] _relatedFileExtensions = new string[] { ".pdb", ".xml" };

        /// <summary>
        /// Gets or sets the related file extensions to copy when <see cref="IncludeRelatedFiles"/> is true.
        /// </summary>
        /// <value>The related file extensions.</value>
        /// <remarks>
        /// The default extensions are .pdb and .xml.
        /// </remarks>
        public string[] RelatedFileExtensions
        {
            get { return _relatedFileExtensions; }
            set { _relatedFileExtensions = value; }
        }


        private bool _includeRelatedFiles = false;

        /// <summary>
        /// Gets or sets a value indicating whether related files are included when installing in GAC.
        /// </summary>
        /// <value><c>true</c> if related files are included when installing in GAC; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Setting IncludeRelatedFiles to true will copy the pdb and xml files from the same folder as the
        /// assembly to the location in the GAC that the assembly was installed to.  This is useful in some  
        /// debugging scenarios were you need to debug assemblies that are GAC'd.
        /// </remarks>
        public bool IncludeRelatedFiles
        {
            get { return _includeRelatedFiles; }
            set { _includeRelatedFiles = value; }
        }

        private bool _quiet = false;

        /// <summary>
        /// Gets or sets a value indicating whether warning messages are output.
        /// </summary>
        /// <value><c>true</c> to not log warning messages; otherwise, <c>false</c>.</value>
        public bool Quiet
        {
            get { return _quiet; }
            set { _quiet = value; }
        }


        private bool _force = false;

        /// <summary>
        /// Gets or sets a value indicating whether to force reinstall of an assembly.
        /// </summary>
        /// <value><c>true</c> if force; otherwise, <c>false</c>.</value>
        public bool Force
        {
            get { return _force; }
            set { _force = value; }
        }

        private string[] _assemblies;


        /// <summary>
        /// Gets or sets the assembly name or file.
        /// </summary>
        /// <value>The assembly name or file.</value>
        /// <remarks>
        /// When the command is install, Assemblies should be a file path to the assembly
        /// to install in the GAC.  When command is uninstall, Assemblies should be a 
        /// the full name of the assembly to uninstall.
        /// </remarks>
        [Required]
        public string[] Assemblies
        {
            get { return _assemblies; }
            set { _assemblies = value; }
        }

        private List<ITaskItem> _installedPaths = new List<ITaskItem>();

        /// <summary>
        /// Gets the installed assembly paths.
        /// </summary>
        /// <value>The installed paths.</value>
        [Output]
        public ITaskItem[] InstalledPaths
        {
            get { return _installedPaths.ToArray(); }
        }

        private List<string> _installedNames = new List<string>();

        /// <summary>
        /// Gets the installed assembly names.
        /// </summary>
        /// <value>The installed names.</value>
        [Output]
        public string[] InstalledNames
        {
            get { return _installedNames.ToArray(); }
        }

        private int _successful = 0;

        /// <summary>
        /// Gets the number of assemblies successfully installed/uninstalled.
        /// </summary>
        /// <value>The number successful assemblies.</value>
        [Output]
        public int Successful
        {
            get { return _successful; }
        }

        private int _failed = 0;

        /// <summary>
        /// Gets the number of assemblies that failed to installed/uninstalled.
        /// </summary>
        /// <value>The number failed assemblies.</value>
        [Output]
        public int Failed
        {
            get { return _failed; }
        }

        private int _skipped = 0;

        /// <summary>
        /// Gets the number of assemblies that were skipped during installed/uninstalled.
        /// </summary>
        /// <value>The number of skipped assemblies.</value>
        [Output]
        public int Skipped
        {
            get { return _skipped; }
        }
        #endregion

        /// <summary>
        /// Runs the executable file with the specified task parameters.
        /// </summary>
        /// <returns>
        /// true if the task runs successfully; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            if ((_assemblies == null) || (_assemblies.Length == 0))
                return true;

            if (_command == GacUtilCommands.Uninstall)
                Uninstall();
            else
                Install();

            Log.LogMessage("GacUtil Successful: {0}  Failed: {1}  Skipped: {2}", _successful, _failed, _skipped);
            return (_failed == 0);
        }

        private void Uninstall()
        {
            foreach (string name in _assemblies)
            {
                try
                {
                    UnintallAssembly(name);
                }
                catch (Exception ex)
                {
                    _failed++;
                    Log.LogErrorFromException(ex, false);
                }
            }
        } // unintall

        private void UnintallAssembly(string name)
        {
            Log.LogMessage("Uninstall: {0}", name);
            
            string fullName;
            string installPath = FusionWrapper.GetAssemblyPath(name, out fullName);

            if (string.IsNullOrEmpty(installPath))
            {
                Log.LogMessage("  Status: {0}", UninstallStatus.ReferenceNotFound.ToString());
                if (!_quiet)
                    Log.LogWarning("Assembly '{0}' not found in the GAC.", name);

                _skipped++;
                return;
            }

            _installedPaths.Add(new TaskItem(installPath));

            AssemblyName assemblyName = AssemblyName.GetAssemblyName(installPath);
            _installedNames.Add(assemblyName.FullName);

            UninstallStatus status = UninstallStatus.None;
            bool result = FusionWrapper.UninstallAssembly(fullName, _force, out status);

            if (result)
                _successful++;
            else
                _failed++;

            Log.LogMessage("  Status: {0}", status.ToString());
        }

        private void Install()
        {
            foreach (string file in _assemblies)
            {
                try
                {
                    InstallFile(file);
                }
                catch (Exception ex)
                {
                    _failed++;
                    Log.LogErrorFromException(ex, false);
                }
            }
        } // Install

        private void InstallFile(string file)
        {
            if (!File.Exists(file))
            {
                Log.LogError("Assembly file '{0}' not found.", file);
                _failed++;
                return;
            }

            AssemblyName name = AssemblyName.GetAssemblyName(file);
            string fullName = FusionWrapper.AppendProccessor(name.FullName, name.ProcessorArchitecture);
            _installedNames.Add(name.FullName);

            FusionWrapper.InstallAssembly(file, _force);
            Log.LogMessage("Installed: {0}", name.FullName);

            string installPath = FusionWrapper.GetAssemblyPath(fullName);
            _installedPaths.Add(new TaskItem(installPath));

            _successful++;

            if (_includeRelatedFiles)
                CopyRelatedFiles(file, Path.GetDirectoryName(installPath));
        }

        private void CopyRelatedFiles(string sourceAssembly, string targetDirectory)
        {
            if (_relatedFileExtensions == null || _relatedFileExtensions.Length == 0)
                return;

            foreach (string extension in _relatedFileExtensions)
            {
                try
                {
                    string relatedFile = Path.ChangeExtension(sourceAssembly, extension);
                    if (!File.Exists(relatedFile))
                        continue;

                    string name = Path.GetFileName(relatedFile);
                    string newFile = Path.Combine(targetDirectory, name);

                    File.Copy(relatedFile, newFile, true);
                    Log.LogMessage("  Copied File: {0}", name);
                }
                catch (Exception ex)
                {
                    Log.LogWarningFromException(ex, false);
                }
            }
        }

    }
}
