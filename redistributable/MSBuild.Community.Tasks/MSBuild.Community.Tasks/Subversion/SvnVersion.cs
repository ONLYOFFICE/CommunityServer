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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;



namespace MSBuild.Community.Tasks.Subversion
{
	/// <summary>
	/// Summarize the local revision(s) of a working copy.
	/// </summary>
	/// <example>The following example gets the revision of the current folder.
	/// <code><![CDATA[
	/// <Target Name="Version">
	///   <SvnVersion LocalPath=".">
	///     <Output TaskParameter="Revision" PropertyName="Revision" />
	///   </SvnVersion>
	///   <Message Text="Revision: $(Revision)"/>
	/// </Target>
	/// ]]></code>
	/// </example>
	public class SvnVersion : ToolTask
	{
		#region Fields
		private static readonly Regex _numberRegex = new Regex(@"\d+", RegexOptions.Compiled);
		private StringBuilder _outputBuffer;

		#endregion Fields

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="T:SvnVersion"/> class.
		/// </summary>
		public SvnVersion()
		{
			_outputBuffer = new StringBuilder();
		}

		#endregion Constructor

		#region Input Parameters
		private string _localPath;

		/// <summary>Path to local working copy.</summary>
		[Required]
		public string LocalPath
		{
			get { return _localPath; }
			set { _localPath = value; }
		}

        private bool _useLastCommittedRevision;

        /// <summary>
        /// Specifies whether to use the last committed revision number as
        /// opposed to the last updated revision number.
        /// </summary>
        public bool UseLastCommittedRevision
        {
            get { return _useLastCommittedRevision; }
            set { _useLastCommittedRevision = value; }
        }

		#endregion Input Parameters

		#region Output Parameters
		/// <summary>Revision number of the local working repository.</summary>
		[Output]
		public int Revision
		{
			get { return _highRevision; }
			set { _highRevision = value; }
		}

		private int _highRevision = -1;

		/// <summary>High revision number of the local working repository revision range.</summary>
		[Output]
		public int HighRevision
		{
			get { return _highRevision; }
			set { _highRevision = value; }
		}

		private int _lowRevision = -1;

		/// <summary>Low revision number of the local working repository revision range.</summary>
		[Output]
		public int LowRevision
		{
			get { return _lowRevision; }
			set { _lowRevision = value; }
		}

		private bool _modifications;

		/// <summary>True if working copy contains modifications.</summary>
		[Output]
		public bool Modifications
		{
			get { return _modifications; }
			set { _modifications = value; }
		}

		private bool _switched;

		/// <summary>True if working copy is switched.</summary>
		[Output]
		public bool Switched
		{
			get { return _switched; }
			set { _switched = value; }
		}

		private bool _exported;

		/// <summary>
		/// True if invoked on a directory that is not a working copy, 
		/// svnversion assumes it is an exported working copy and prints "exported".
		/// </summary>
		[Output]
		public bool Exported
		{
			get { return _exported; }
			set { _exported = value; }
		}

		#endregion Output Parameters


		#region Task Overrides
		/// <summary>
		/// Returns the fully qualified path to the executable file.
		/// </summary>
		/// <returns>
		/// The fully qualified path to the executable file.
		/// </returns>
		protected override string GenerateFullPathToTool()
		{
            string path = SvnClient.FindToolPath(ToolName);
            base.ToolPath = path;

            return Path.Combine(ToolPath, ToolName);
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
		/// Gets the name of the executable file to run.
		/// </summary>
		/// <value></value>
		/// <returns>The name of the executable file to run.</returns>
		protected override string ToolName
		{
			get { return "svnversion.exe"; }
		}

		/// <summary>
		/// Returns a string value containing the command line arguments to pass directly to the executable file.
		/// </summary>
		/// <returns>
		/// A string value containing the command line arguments to pass directly to the executable file.
		/// </returns>
        protected override string GenerateCommandLineCommands()
        {
            DirectoryInfo localPath = new DirectoryInfo(_localPath);
            string commandLineFormat = _useLastCommittedRevision ? "-c --no-newline \"{0}\"" : "--no-newline \"{0}\"";
            return string.Format(commandLineFormat, localPath.FullName.Replace('\\', '/'));
        }

		/// <summary>
		/// Runs the exectuable file with the specified task parameters.
		/// </summary>
		/// <returns>
		/// true if the task runs successfully; otherwise, false.
		/// </returns>
		public override bool Execute()
		{
			bool result = base.Execute();
			if (result)
			{
				ParseOutput();
			}
			return result;
		}

		/// <summary>
		/// Logs the events from text output.
		/// </summary>
		/// <param name="singleLine">The single line.</param>
		/// <param name="messageImportance">The message importance.</param>
		protected override void LogEventsFromTextOutput(string singleLine, Microsoft.Build.Framework.MessageImportance messageImportance)
		{
			base.LogEventsFromTextOutput(singleLine, messageImportance);
			_outputBuffer.Append(singleLine);
		}

		#endregion Task Overrides

		#region Private Methods
		private void ParseOutput()
		{
			string buffer = _outputBuffer.ToString();
			MatchCollection revisions = _numberRegex.Matches(buffer);
			foreach (Match rm in revisions)
			{
				int revision;
				if (int.TryParse(rm.Value, out revision))
				{
					if (_lowRevision == -1)
					{
					  _lowRevision = revision;
					}
					_lowRevision = System.Math.Min(revision, _lowRevision);
					_highRevision = System.Math.Max(revision, _highRevision);
				}
			}

			_modifications = buffer.Contains("M");
			_switched = buffer.Contains("S");
			_exported = buffer.Contains("exported");
			if (_exported)
				Log.LogWarning(Properties.Resources.SvnLocalPathNotWorkCopy);

		}

		#endregion Private Methods

	}
}
