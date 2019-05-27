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
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Win32;



namespace MSBuild.Community.Tasks.Subversion
{

    /// <summary>
    /// Subversion client base class
    /// </summary>
    public class SvnClient : ToolTask
    {
        #region Fields
        private const string _switchBooleanFormat = " --{0}";
        private const string _switchStringFormat = " --{0} \"{1}\"";
        private const string _switchValueFormat = " --{0} {1}";

        private static readonly Regex _revisionParse = new Regex(@"(?<=[rR]ev(ision)?\s+)\d+", RegexOptions.Compiled);

        private StringBuilder _outputBuffer = new StringBuilder();
        private StringBuilder _errorBuffer = new StringBuilder();

        private static readonly string _sanitizedPasswordArgument = string.Format(_switchValueFormat, "password", "***");
        private string _passwordArgument;

        #endregion Fields

        #region Input Parameters
        private string _command;

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>The command.</value>
        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }

        private string _arguments;

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        public string Arguments
        {
            get { return _arguments; }
            set { _arguments = value; }
        }

        private string _username;

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        private string _password;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private bool _sanitizePassword;

        /// <summary>
        /// Allows to sanitize password string from svn command log output.
        /// </summary>
        /// <value>The sanitize.</value>
        public bool SanitizePassword 
        {
            get { return _sanitizePassword; }
            set { _sanitizePassword = value; }
        }

        private bool _verbose;

        /// <summary>
        /// Gets or sets the verbose.
        /// </summary>
        /// <value>The verbose.</value>
        public bool Verbose
        {
            get { return _verbose; }
            set { _verbose = value; }
        }

        private bool _force;

        /// <summary>
        /// Gets or sets the force.
        /// </summary>
        /// <value>The force.</value>
        public bool Force
        {
            get { return _force; }
            set { _force = value; }
        }

        private string _message;

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        private string _messageFile;

        /// <summary>
        /// Gets or sets the message file.
        /// </summary>
        /// <value>The message file.</value>
        /// <remarks>
        /// Uses the contents of the named file for the specified 
        /// subcommand, though different subcommands do different 
        /// things with this content.</remarks>
        public string MessageFile
        {
            get { return _messageFile; }
            set { _messageFile = value; }
        }

        private string _repositoryPath;

        /// <summary>
        /// Gets or sets the repository path.
        /// </summary>
        /// <value>The repository path.</value>
        [Output]
        public string RepositoryPath
        {
            get { return _repositoryPath; }
            set { _repositoryPath = value; }
        }

        private string _localPath;

        /// <summary>
        /// Gets or sets the local path.
        /// </summary>
        /// <value>The local path.</value>
        public string LocalPath
        {
            get { return _localPath; }
            set { _localPath = value; }
        }

        private string _targetFile;

        /// <summary>
        /// Gets or sets the target file.
        /// </summary>
        /// <value>The target file.</value>
        /// <remarks>
        /// Tells Subversion to get the list of files that you wish to operate on from
        /// the filename that you provide instead of listing all the files on the command line.
        ///  </remarks>
        public string TargetFile
        {
            get { return _targetFile; }
            set { _targetFile = value; }
        }

        private ITaskItem[] _targets;

        /// <summary>
        /// Gets or sets the targets.
        /// </summary>
        /// <value>The targets.</value>
        public ITaskItem[] Targets
        {
            get { return _targets; }
            set { _targets = value; }
        }

        private bool _nonInteractive = true;
        /// <summary>
        /// Gets or sets a value indicating the command is non interactive].
        /// </summary>
        /// <value><c>true</c> if non interactive; otherwise, <c>false</c>.</value>
        public bool NonInteractive
        {
            get { return _nonInteractive; }
            set { _nonInteractive = value; }
        }

        private bool _noAuthCache = true;
        /// <summary>
        /// Gets or sets a value indicating no auth cache.
        /// </summary>
        /// <value><c>true</c> if no auth cache; otherwise, <c>false</c>.</value>
        public bool NoAuthCache
        {
            get { return _noAuthCache; }
            set { _noAuthCache = value; }
        }

        private bool _trustServerCert;
        /// <summary>
        /// Gets or sets a value indicating whether to trust the server cert.
        /// </summary>
        /// <value><c>true</c> to trust the server cert; otherwise, <c>false</c>.</value>
        public bool TrustServerCert
        {
            get { return _trustServerCert; }
            set { _trustServerCert = value; }
        }

        private bool _xml;
        /// <summary>
        /// Gets or sets a value indicating the output is XML.
        /// </summary>
        /// <value><c>true</c> to output in XML; otherwise, <c>false</c>.</value>
        public bool Xml
        {
            get { return _xml; }
            set { _xml = value; }
        }

        #endregion Input Parameters

        #region Output Parameters
        private int _revision = -1;

        /// <summary>
        /// Gets or sets the revision.
        /// </summary>
        /// <value>The revision.</value>
        [Output]
        public int Revision
        {
            get { return _revision; }
            set { _revision = value; }
        }

        /// <summary>
        /// Gets the output of SVN command-line client.
        /// </summary>
        [Output]
        public string StandardOutput
        {
            get { return _outputBuffer.ToString(); }
        }

        /// <summary>
        /// Gets the error output of SVN command-line client.
        /// </summary>
        [Output]
        public string StandardError
        {
            get { return _errorBuffer.ToString(); }
        }

        #endregion Output Parameters

        #region Protected Methods
        /// <summary>
        /// Generates the SVN command.
        /// </summary>
        /// <returns></returns>
        protected virtual string GenerateSvnCommand()
        {
            StringBuilder builder = new StringBuilder();


            builder.Append(_command);

            if (!string.IsNullOrEmpty(_repositoryPath))
                builder.AppendFormat(" \"{0}\"", _repositoryPath);

            if (!string.IsNullOrEmpty(_localPath))
                builder.AppendFormat(" \"{0}\"", _localPath);

            if (_revision >= 0)
                builder.AppendFormat(_switchValueFormat, "revision", _revision);

            if (_targets != null)
            {
                foreach (ITaskItem fileTarget in _targets)
                {
                    builder.AppendFormat(" \"{0}\"", fileTarget.ItemSpec);
                }
            }

            if (!string.IsNullOrEmpty(_targetFile))
                builder.AppendFormat(_switchStringFormat, "targets", _targetFile);

            return builder.ToString();
        }

        /// <summary>
        /// Generates the SVN arguments.
        /// </summary>
        /// <returns></returns>
        protected virtual string GenerateSvnArguments()
        {

            StringBuilder builder = new StringBuilder();

            if (!string.IsNullOrEmpty(_username))
                builder.AppendFormat(_switchValueFormat, "username", _username);

            if (!string.IsNullOrEmpty(_password))
                builder.AppendFormat(_switchValueFormat, "password", _password);

            if (!string.IsNullOrEmpty(_messageFile) && File.Exists(_messageFile))
                builder.AppendFormat(_switchStringFormat, "file", _messageFile);

            if (!string.IsNullOrEmpty(_message))
                builder.AppendFormat(_switchStringFormat, "message", _message);

            if (_force)
                builder.AppendFormat(_switchBooleanFormat, "force");

            if (_verbose)
                builder.AppendFormat(_switchBooleanFormat, "verbose");

            if (!string.IsNullOrEmpty(_arguments))
                builder.AppendFormat(" {0}", _arguments);

            if (_xml)
                builder.AppendFormat(_switchBooleanFormat, "xml");

            if (_nonInteractive)
                builder.AppendFormat(_switchBooleanFormat, "non-interactive");

            if (_noAuthCache)
                builder.AppendFormat(_switchBooleanFormat, "no-auth-cache");

            if (_trustServerCert)
                builder.AppendFormat(_switchBooleanFormat, "trust-server-cert");

            return builder.ToString();
        }

        #endregion Protected Methods

        #region Task Overrides
        /// <summary>
        /// Runs the exectuable file with the specified task parameters.
        /// </summary>
        /// <returns>
        /// true if the task runs successfully; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            _outputBuffer.Length = 0; // clear buffer
            _errorBuffer.Length = 0;

            return base.Execute();
        }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            return GenerateSvnCommand() + GenerateSvnArguments();
        }

        /// <summary>
        /// Indicates whether all task paratmeters are valid.
        /// </summary>
        /// <returns>
        /// true if all task parameters are valid; otherwise, false.
        /// </returns>
        protected override bool ValidateParameters()
        {
            if (string.IsNullOrEmpty(_command))
            {
                Log.LogError(Properties.Resources.ParameterRequired, "SvnClient", "Command");
                return false;
            }
            return base.ValidateParameters();
        }

        /// <summary>
        /// Logs the events from text output.
        /// </summary>
        /// <param name="singleLine">The single line.</param>
        /// <param name="messageImportance">The message importance.</param>
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            bool isError = messageImportance == StandardErrorLoggingImportance;
            
            // dont log xml messages
            if (!Xml || isError)
                base.LogEventsFromTextOutput(singleLine, messageImportance);

            // keep error output and standard output seperated. 
            // only way to tell is the message importance.
            if (isError)
            {
                _errorBuffer.AppendLine(singleLine);
                return; // no need to continue if error
            }
            
            _outputBuffer.AppendLine(singleLine);

            Match revMatch = _revisionParse.Match(singleLine);
            if (revMatch.Success)
                int.TryParse(revMatch.Value, out _revision);
        }

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            if (string.IsNullOrEmpty(ToolPath))
                ToolPath = FindToolPath(ToolName);

            return Path.Combine(ToolPath, ToolName);
        }

        /// <summary>
        /// Finds the tool path.
        /// </summary>
        /// <param name="toolName">Name of the tool.</param>
        /// <returns></returns>
        public static string FindToolPath(string toolName)
        {
            string toolPath =
                ToolPathUtil.FindInRegistry(toolName) ??
                ToolPathUtil.FindInPath(toolName) ??
                ToolPathUtil.FindInProgramFiles(toolName,
                    @"Subversion\bin",
                    @"CollabNet Subversion Server",
                    @"CollabNet Subversion",
                    @"CollabNet Subversion Client",
                    @"VisualSVN\bin",
                    @"VisualSVN Server\bin",
                    @"SlikSvn\bin");

            if (toolPath == null)
            {
                throw new Exception("Could not find svn.exe.  Looked in PATH locations and various common folders inside Program Files.");
            }

            return toolPath;
        }

        /// <summary>
        /// Logs the starting point of the run to all registered loggers.
        /// </summary>
        /// <param name="message">A descriptive message to provide loggers, usually the command line and switches.</param>
        protected override void LogToolCommand(string message)
        {
            if (SanitizePassword && !string.IsNullOrEmpty(message)) {
                if (_passwordArgument == null) {
                    _passwordArgument = string.Format(_switchValueFormat, "password", _password);
                }

                message = message.Replace(_passwordArgument, _sanitizedPasswordArgument);
            }
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
        /// Gets the <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.</returns>
        protected override MessageImportance StandardErrorLoggingImportance
        {
            get { return MessageImportance.High; }
        }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the executable file to run.</returns>
        protected override string ToolName
        {
            get { return ToolPathUtil.MakeToolName("svn"); }
        }

        #endregion Task Overrides

    }
}
