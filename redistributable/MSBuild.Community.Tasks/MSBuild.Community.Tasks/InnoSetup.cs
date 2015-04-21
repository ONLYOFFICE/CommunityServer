// Copyright � 2008 Ivan Gonzalez

using System;
using System.IO;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// MSBuild task to create installer with InnoSetup
    /// </summary>
    /// <example>Create installer
    /// <code><![CDATA[
    ///     <InnoSetup 
    ///         ScriptFile="setup.iss"
    ///         OutputFileName="MySetup.exe"
    ///         OutputPath="C:\SetupDir"
    ///         Quiet="True" />
    /// ]]></code>
    /// </example>
    public class InnoSetup : ToolTask
    {
        #region Properties
        private string _scriptFile;

        /// <summary>
        /// Filename of Inno Setup script (.iss)
        /// </summary>
        /// <value>InnoSetup file, example: C:\Setup.iss</value>
        [Required]
        public string ScriptFile
        {
            get { return _scriptFile; }
            set
            {
                _scriptFile = value;
            }
        }

        private string _outputFileName;

        /// <summary>
        /// Specify output filename
        /// </summary>
        /// <value>Name for setup, examples: MySetup.exe</value>
        public string OutputFileName
        {
            get { return _outputFileName; }

            set
            {
                _outputFileName = value;
            }
        }

        private string _outputPath;

        /// <summary>
        /// Specify output path
        /// </summary>
        /// <value>Path for output setup, example: C:\Setups</value>
        public string OutputPath
        {
            get { return _outputPath; }
            set
            {
                _outputPath = value;
            }
        }

        private string _quiet;
        /// <summary>
        /// Quiet compile
        /// </summary>
        /// <value>True o False</value>
        public string Quiet
        {
            get {
                return _quiet;
            }

            set {
                _quiet = value;
            }
        }
        #endregion

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), string.Concat(@"Inno Setup 5\", this.ToolName));
        }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the executable file to run.</returns>
        protected override string ToolName
        {
            get { return "ISCC.exe"; }
        }

        /// <summary>
        /// Logs the starting point of the run to all registered loggers.
        /// </summary>
        /// <param name="message">A descriptive message to provide loggers, usually the command line and switches.</param>
        protected override void LogToolCommand(string message)
        {
            Log.LogCommandLine(MessageImportance.Low, message);
        }

        /// <summary>
        /// Gets the <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.</returns>
        protected override MessageImportance StandardOutputLoggingImportance
        {
            get { return MessageImportance.Normal; }
        }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();
            builder.AppendSwitch("\"" + this._scriptFile + "\"");

            if (string.IsNullOrEmpty(this._outputFileName) == false) {
                builder.AppendSwitch(string.Concat("/F", "\"" + this._outputFileName + "\""));
            }

            if (string.IsNullOrEmpty(this._outputPath) == false) {
                builder.AppendSwitch(string.Concat("/O", "\"" + this._outputPath + "\""));
            }

            if (string.IsNullOrEmpty(this._quiet) == false) {
                if (this._quiet == "True") {
                    builder.AppendSwitch("/Q");
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            if (File.Exists(this.GenerateFullPathToTool()) == false) {
                Log.LogError("InnoSetup compiler can't be found");
                return false;
            }

            return base.Execute();
        }
    }
}
