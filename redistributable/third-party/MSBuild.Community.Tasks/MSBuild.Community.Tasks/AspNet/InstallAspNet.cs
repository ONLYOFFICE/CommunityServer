
using System;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.AspNet
{
    /// <summary>
    /// Installs and register script mappings for ASP.NET
    /// </summary>
    /// <remarks>Uses the aspnet_regiis.exe tool included with the .NET Framework.</remarks>
    /// <include file='..\AdditionalDocumentation.xml' path='docs/task[@name="InstallAspNet"]/*'/>
    public class InstallAspNet : ToolTask
    {
        private enum ScriptMapScenarios
        {
            Unspecified,
            Never,
            IfNoneExist,
            UnlessNewerExist,
            Always
        }

        bool validArguments = true;

        private TargetDotNetFrameworkVersion version = TargetDotNetFrameworkVersion.VersionLatest;

        /// <summary>
        /// The version of ASP.NET to install
        /// </summary>
        /// <remarks>
        /// The default behavior is to use the latest version of ASP.NET available on the computer.
        /// <list type="table">
        /// <listheader><term>Version</term></listheader>
        /// <item><term>Version11</term><description>ASP.NET v1.1</description></item>
        /// <item><term>Version20</term><description>ASP.NET v2.0</description></item>
        /// <item><term>VersionLatest</term><description>The latest version of ASP.NET available</description></item>
        /// </list>
        /// </remarks>
        public string Version
        {
            get { return version.ToString(); }
            set
            {
                try
                {
                    version = (TargetDotNetFrameworkVersion)Enum.Parse(version.GetType(), value);
                }
                catch(ArgumentException){
                    Log.LogError("The Version '{0}' is invalid. Valid values are {1}", value, String.Join(", ", Enum.GetNames(version.GetType())));
                    validArguments = false;
                }
            }
        }

        private ScriptMapScenarios applyScriptMaps = ScriptMapScenarios.Unspecified;

        /// <summary>
        /// The method used to determine if ASP.NET script mappings should be applied
        /// </summary>
        /// <remarks>
        /// The default behavior is to register script mappings on all sites except those with a newer version of ASP.NET.
        /// <list type="table">
        /// <listheader><term>Value</term></listheader>
        /// <item><term>Never</term><description>Register ASP.NET on the computer without updating any script mappings.</description></item>
        /// <item><term>IfNoneExist</term><description>Register script mappings only on for sites that do not have any existing ASP.NET script mappings (not available for ASP.NET v1.1)</description></item>
        /// <item><term>UnlessNewerExist</term><description>Register script mappings on all sites except those with a newer version of ASP.NET.</description></item>
        /// <item><term>Always</term><description>Register script mappings on all sites, even if they already have a newer version of ASP.NET.</description></item>
        /// </list>
        /// </remarks>
        public string ApplyScriptMaps
        {
            get { return applyScriptMaps.ToString(); }
            set
            {
                try
                {
                    applyScriptMaps = (ScriptMapScenarios)Enum.Parse(applyScriptMaps.GetType(), value);
                }
                catch (ArgumentException)
                {
                    Log.LogError("The ApplyScriptMaps value '{0}' is invalid. Valid values are {1}", value, String.Join(", ", Enum.GetNames(applyScriptMaps.GetType())));
                    validArguments = false;
                }
            }
        }

        private bool clientScriptsOnly = false;

        /// <summary>
        /// When <see langword="true" />, the aspnet_client scripts will be installed. No script mappings will be updated.
        /// </summary>
        /// <remarks>This cannot be <see langword="true" /> if a value for <see cref="Path"/> or <see cref="ApplyScriptMaps"/> has been specified.</remarks>
        public bool ClientScriptsOnly
        {
            get { return clientScriptsOnly; }
            set { clientScriptsOnly = value; }
        }


        private string path = null;

        /// <summary>
        /// The web application that should have its script maps updated.
        /// </summary>
        /// <remarks>
        /// The path must be of the form W3SVC/[instance]/Root/[webdirectory], for example W3SVC/1/Root/SampleApp1.
        /// As a shortcut, you can specify just the web directory name,
        /// if the web directory is installed in the default website instance (W3SVC/1/Root).
        /// <para>You should not specify a value for <see cref="ApplyScriptMaps"/> when specifying a path.</para>
        /// </remarks>
        public string Path
        {
            get { return path; }
            set
            {
                if (!value.Contains(@"/"))
                {
                    path = "W3SVC/1/Root/" + value;
                }
                else
                {
                    path = value;
                }
                if (!path.StartsWith(@"W3SVC/", StringComparison.InvariantCultureIgnoreCase))
                {
                    Log.LogError(@"Path must be in the form W3SVC/1/Root/SampleApp1.");
                    validArguments = false;
                }
            }
        }

        private bool recursive = true;

        /// <summary>
        /// When <see langword="true" />, script maps are applied recursively under <see cref="Path"/>.
        /// </summary>
        /// <remarks>This property is only valid when specifying a value for <see cref="Path"/>. It is <see langword="true" /> by default.</remarks>
        public bool Recursive
        {
            get { return recursive; }
            set { recursive = value; }
        }

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
                return ToolLocationHelper.GetPathToDotNetFrameworkFile(ToolName, version);
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
            get { return "AspNet_RegIIS.exe"; }
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
            if (clientScriptsOnly)
            {
                Log.LogMessage("Installing ASP.NET client side scripts.");
                commandLine.AppendSwitch("-c");
            }
            else if (path != null)
            {
                if (recursive)
                {
                    Log.LogMessage("Installing ASP.NET script maps recursively on {0}", path);
                    commandLine.AppendSwitch("-s " + path);
                }
                else
                {
                    Log.LogMessage("Installing ASP.NET script maps non-recursively on {0}", path);
                    commandLine.AppendSwitch("-sn " + path);
                }
            }
            else
            {
                switch (applyScriptMaps)
                {
                    case ScriptMapScenarios.Never:
                        Log.LogMessage("Installing ASP.NET without updating script maps.");
                        commandLine.AppendSwitch("-ir");
                        break;
                    case ScriptMapScenarios.IfNoneExist:
                        Log.LogMessage("Installing ASP.NET and only updating script maps if they do not already exist.");
                        commandLine.AppendSwitch("-iru");
                        break;
                    case ScriptMapScenarios.Unspecified:
                    case ScriptMapScenarios.UnlessNewerExist:
                        Log.LogMessage("Installing ASP.NET and updating script maps unless a newer version exists.");
                        commandLine.AppendSwitch("-i");
                        break;
                    case ScriptMapScenarios.Always:
                        Log.LogMessage("Installing ASP.NET and updating script maps (overriding any newer versions).");
                        commandLine.AppendSwitch("-r");
                        break;
                    default:
                        throw new ArgumentException("ApplyScriptMaps");
                }
            }
            return commandLine.ToString();
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// True if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            if (!IsValidPropertyCombinations())
            {
                return false;
            }
            return base.Execute();
        }

        /// <summary>
        /// Determines if the current property values can be used together
        /// </summary>
        /// <returns><see langword="true"/> when properties can be used together.</returns>
        /// <exclude />
        public bool IsValidPropertyCombinations()
        {
            if (clientScriptsOnly)
            {
                if (path != null)
                {
                    Log.LogError("You cannot specify a value for Path when installing client scripts only.");
                    validArguments = false;
                }
                if (applyScriptMaps != ScriptMapScenarios.Unspecified)
                {
                    Log.LogError("You cannot apply script maps when installing client scripts only.");
                    validArguments = false;
                }
                if (!recursive)
                {
                    Log.LogError("Client scripts must always be applied recursively.");
                    validArguments = false;
                }
            }
            if (path != null)
            {
                if ((applyScriptMaps == ScriptMapScenarios.Never) || (applyScriptMaps == ScriptMapScenarios.UnlessNewerExist))
                {
                    Log.LogError("Script maps for the current version must be installed when you specify a Path.");
                    validArguments = false;
                }
            }
            else
            {
                if (!recursive)
                {
                    Log.LogError("Script maps must be applied recursively unless you specify a Path.");
                    validArguments = false;
                }
            }
            if (applyScriptMaps == ScriptMapScenarios.IfNoneExist && version == TargetDotNetFrameworkVersion.Version11)
            {
                Log.LogError("The ApplyScriptMaps=IfNoneExists option is not available for ASP.NET v1.1.");
                validArguments = false;
            }
            return validArguments;
        }
    }
}
