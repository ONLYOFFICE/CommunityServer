//$Id$
using System;
using Microsoft.Build.Framework;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.IIS
{
    /// <summary>
    /// Sets an application mapping for a filename extension on an existing web directory.
    /// </summary>
    /// <example>Map the .axd extension to the lastest version of ASP.NET:
    /// <code><![CDATA[
    /// <WebDirectoryScriptMap VirtualDirectoryName="MyWeb" Extension=".axd" MapToAspNet="True" VerifyFileExists="False" />
    /// ]]></code>
    /// </example>
    /// <example>Map GET requests to the .rss extension to a specific executable:
    /// <code><![CDATA[
    /// <WebDirectoryScriptMap VirtualDirectoryName="MyWeb" Extension=".rss" Verbs="GET" ExecutablePath="$(WINDIR)\Microsoft.Net\Framework\1.0.3705\aspnet_isapi.dll" />
    /// ]]></code>
    /// </example>
    public class WebDirectoryScriptMap : WebBase
    {
        private string mVirtualDirectoryName;
        /// <summary>
        /// Gets or sets the name of the virtual directory.
        /// </summary>
        /// <value>The name of the virtual directory.</value>
        [Required]
        public string VirtualDirectoryName
        {
            get
            {
                return mVirtualDirectoryName;
            }
            set
            {
                mVirtualDirectoryName = value;
            }
        }

        private string extension;
        /// <summary>
        /// The filename extension that will be mapped to an executable.
        /// </summary>
        [Required]
        public string Extension
        {
            get { return extension; }
            set { extension = value; }
        }

        private string executablePath;

        /// <summary>
        /// The full path to the executable used to respond to requests for a Uri ending with <see cref="Extension"/>
        /// </summary>
        /// <remarks>This property is required when <see cref="MapToAspNet"/> is <c>false</c> (the default).</remarks>
        public string ExecutablePath
        {
            get { return executablePath; }
            set { executablePath = value; }
        }

        private bool mapToAspNet;

        /// <summary>
        /// Indicates whether <see cref="Extension"/> should be mapped to the ASP.NET runtime.
        /// </summary>
        /// <remarks>When <c>true</c>, <see cref="ExecutablePath"/> is set to aspnet_isapi.dll
        /// in the installation folder of the latest version of the .NET Framework.</remarks>
        public bool MapToAspNet
        {
            get { return mapToAspNet; }
            set { mapToAspNet = value; }
        }

        private string verbs;

        /// <summary>
        /// A comma-separated list of the HTTP verbs to include in the application mapping.
        /// </summary>
        /// <remarks>The default behavior (when this property is empty or unspecified) is to map all verbs.
        /// <para>A semi-colon-separated list will also be recognized, allowing you to use an MSBuild Item.</para></remarks>
        public string Verbs
        {
            get { return verbs; }
            set { verbs = value; }
        }

        private bool enableScriptEngine;

        /// <summary>
        /// Set to <c>true</c> when you want the application to run in a directory without Execute permissions.
        /// </summary>
        public bool EnableScriptEngine
        {
            get { return enableScriptEngine; }
            set { enableScriptEngine = value; }
        }

        private bool verifyFileExists;

        /// <summary>
        /// Set to <c>true</c> to instruct the Web server to verify the existence of the requested script file and to ensure that the requesting user has access permission for that script file.
        /// </summary>
        public bool VerifyFileExists
        {
            get { return verifyFileExists; }
            set { verifyFileExists = value; }
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// True if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            if (!mapToAspNet && String.IsNullOrEmpty(executablePath))
            {
                Log.LogError("You must either specify a value for ExecutablePath, or set MapToAspNet = True.");
                return false;
            }
            if (mapToAspNet)
            {
                executablePath = ToolLocationHelper.GetPathToDotNetFrameworkFile("aspnet_isapi.dll", TargetDotNetFrameworkVersion.VersionLatest);
                enableScriptEngine = true;
            }
            int flags = 0;
            if (enableScriptEngine) flags += 1;
            if (verifyFileExists) flags += 4;
            if (!extension.StartsWith(".") && extension != "*") extension = "." + extension;

            DirectoryEntry targetDirectory = null;
            try
            {
                Log.LogMessage(MessageImportance.Normal, Properties.Resources.WebDirectoryScriptMapUpdate, Extension, VirtualDirectoryName, ServerName);

                VerifyIISRoot();

                string targetDirectoryPath = (VirtualDirectoryName == "/")
                    ? targetDirectoryPath = IISServerPath
                    : targetDirectoryPath = IISServerPath + "/" + VirtualDirectoryName;

                targetDirectory = new DirectoryEntry(targetDirectoryPath);

                try
                {
                    string directoryExistsTest = targetDirectory.SchemaClassName;
                }
                catch (COMException)
                {
                    Log.LogError(Properties.Resources.WebDirectoryInvalidDirectory, VirtualDirectoryName, ServerName);
                    return false;
                }

                PropertyValueCollection scriptMaps = targetDirectory.Properties["ScriptMaps"];
                if (scriptMaps == null)
                {
                    Log.LogError(Properties.Resources.WebDirectorySettingInvalidSetting, VirtualDirectoryName, ServerName, "ScriptMaps");
                    return false;
                }

                int extensionIndex = -1;
                for (int i = 0; i < scriptMaps.Count; i++)
                {
                    string scriptMap = scriptMaps[i] as string;
                    if (scriptMap == null) continue;

                    if (scriptMap.StartsWith(extension + ",", StringComparison.InvariantCultureIgnoreCase))
                    {
                        extensionIndex = i;
                        break;
                    }
                }

                string newVerbs = !String.IsNullOrEmpty(verbs) ?
                    "," + verbs.Replace(';', ',') :
                    String.Empty;
                string mappingDetails = String.Format("{0},{1},{2}{3}",
                    extension,
                    executablePath,
                    flags.ToString(),
                    newVerbs
                );

                if (extensionIndex >= 0)
                {
                    scriptMaps[extensionIndex] = mappingDetails;
                }
                else
                {
                    scriptMaps.Add(mappingDetails);
                }
                targetDirectory.CommitChanges();
            }
            finally
            {
                if (targetDirectory != null) targetDirectory.Dispose();
            }
            return true;
        }
    }
}
