#region Copyright © 2008 Paul Welter. All rights reserved.
/*
Copyright © 2008 Paul Welter. All rights reserved.

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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;
using System.Collections.Specialized;



namespace MSBuild.Community.Tasks.Sandcastle
{
    /// <summary>
    /// A base class for Sandcastle Tools,
    /// </summary>
    public abstract class SandcastleToolBase : ToolTask
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SandcastleToolBase"/> class.
        /// </summary>
        public SandcastleToolBase()
        {
            EnviromentVariables = new StringDictionary();
            SandcastleEnviroment = new SandcastleEnviroment();

            EnviromentVariables["DXROOT"] = SandcastleEnviroment.SandcastleRoot;
        }

        /// <summary>
        /// Gets or sets the sandcastle enviroment.
        /// </summary>
        /// <value>The sandcastle enviroment.</value>
        internal SandcastleEnviroment SandcastleEnviroment { get; set; }

        /// <summary>
        /// Gets or sets the enviroment variables.
        /// </summary>
        /// <value>The enviroment variables.</value>
        internal StringDictionary EnviromentVariables { get; set; }

        /// <summary>
        /// Gets or sets the sandcastle install root directory.
        /// </summary>
        /// <value>The sandcastle root directory.</value>
        public string SandcastleRoot
        {
            get
            {
                return SandcastleEnviroment.SandcastleRoot;
            }
            set
            {
                SandcastleEnviroment = new SandcastleEnviroment(value);
                EnviromentVariables["DXROOT"] = value;
            }
        }

        private bool _noInfoMessages;

        /// <summary>
        /// Gets or sets a value indicating whether no info messages will be output.
        /// </summary>
        /// <value><c>true</c> if no info messages; otherwise, <c>false</c>.</value>
        public bool NoInfoMessages
        {
            get { return _noInfoMessages; }
            set { _noInfoMessages = value; }
        }

        private bool _noWarnMessages;

        /// <summary>
        /// Gets or sets a value indicating whether no warning messages will be output.
        /// </summary>
        /// <value><c>true</c> if no warning messages; otherwise, <c>false</c>.</value>
        public bool NoWarnMessages
        {
            get { return _noWarnMessages; }
            set { _noWarnMessages = value; }
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
                ToolPath = SandcastleEnviroment.ToolsDirectory;

            if (Directory.Exists(ToolPath))
                return Path.Combine(ToolPath, ToolName);

            return ToolName;
        }

        /// <summary>
        /// Logs the starting point of the run to all registered loggers.
        /// </summary>
        /// <param name="message">A descriptive message to provide loggers, usually the command line and switches.</param>
        protected override void LogToolCommand(string message)
        {
#if DEBUG
            Log.LogCommandLine(MessageImportance.Normal, message);
#else
            Log.LogCommandLine(MessageImportance.Low, message);
#endif
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
        /// Gets the override value of the PATH environment variable.
        /// </summary>
        /// <value></value>
        /// <returns>The override value of the PATH environment variable.</returns>
        protected override StringDictionary EnvironmentOverride
        {
            get
            {
                return EnviromentVariables;
            }
        }

        /// <summary>
        /// Logs the events from text output.
        /// </summary>
        /// <param name="singleLine">The single line.</param>
        /// <param name="messageImportance">The message importance.</param>
        protected override void LogEventsFromTextOutput(string singleLine, Microsoft.Build.Framework.MessageImportance messageImportance)
        {
            if (messageImportance != MessageImportance.High)
            {
                if (NoInfoMessages && singleLine.StartsWith("Info:", 
                    StringComparison.OrdinalIgnoreCase))
                    return;
                else if (NoWarnMessages && singleLine.StartsWith("Warn:", 
                    StringComparison.OrdinalIgnoreCase))
                    return;
            }

            base.LogEventsFromTextOutput(singleLine, messageImportance);
        }

    }
}
