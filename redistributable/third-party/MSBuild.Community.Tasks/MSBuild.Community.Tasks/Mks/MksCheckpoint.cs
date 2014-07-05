#region Copyright © 2006 Doug Ramirez. All rights reserved.
/*
 Copyright © 2006 Doug Ramirez. All rights reserved.

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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.Mks
{
	/*---------------------------------------------------------------------------------------------------------------*/
	/// <summary>
	///		Custom MSBuild task that calls the MKS Checkpoint command.
	/// </summary>
	/*---------------------------------------------------------------------------------------------------------------*/
	public class MksCheckpoint : ToolTask
	{
		#region Constants
		const string COMMAND = "checkpoint";
		#endregion Constants

		#region Private Member Variables
		private string _description;
		private string _directory;
		private bool   _forceConfirm;
		#endregion Private Member Variables

		#region Contructors
		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Contructor that initializes ToolTask properties.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		public MksCheckpoint()
		{
			// Set the path to the MKS CLI executable
			ToolPath = @"C:\Program Files\MKS\IntegrityClient\bin";
		}
		#endregion Constructors

		#region Protected Properties
		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		The name of the program for the MKS CLI executable.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		protected override string ToolName
		{
			get { return "si.exe"; }
		}
		#endregion Protected Properties

		#region Public Properties
		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Returns the text for the MKS Checkpoint CLI command.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		public string CommandText
		{
			get { return GenerateFullPathToTool() + " " + GenerateCommandLineCommands(); }
		}

		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		The description of the changes associated with the project to Checkpoint.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		[Required]
		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		The directory for the Checkpoint to be executed in.  Any files and members in the selection are treated 
		///		as being relative to that directory.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		[Required]
		public string Directory
		{
			get { return _directory; }
			set { _directory = value; }
		}

		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Control the responses of either "yes" or "no" to all prompts.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		[Required]
		public bool ForceConfirm
		{
			get { return _forceConfirm; }
			set { _forceConfirm = value; }
		}
		#endregion Public Properties

		#region Protected Methods
		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Returns a string value containing the command line arguments to pass directly to the executable file.
		/// </summary>
		/// <returns>
		///		A string value containing the command line arguments to pass directly to the executable file.
		/// </returns>
		/*---------------------------------------------------------------------------------------------------------------*/
		protected override string GenerateCommandLineCommands()
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();

				// Ensure that each string praram has a value.
				if (string.IsNullOrEmpty(_description)
				||  string.IsNullOrEmpty(_directory))
				{
					throw new ArgumentNullException();
				}

				// Build the CLI command to execute an MKS Checkpoint
				stringBuilder.AppendFormat(COMMAND);
				stringBuilder.AppendFormat(" --description={0}", _description);
				stringBuilder.AppendFormat(" --cwd={0}", _directory);

				if (_forceConfirm)
				{
					stringBuilder.AppendFormat(" --forceConfirm=yes");
				}
				else
				{
					stringBuilder.AppendFormat(" --forceConfirm=no");
				}

				return stringBuilder.ToString();
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
				return null;
			}
		}
		#endregion Protected Methods

		#region Public Methods
		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Returns the fully qualified path to the executable file.
		/// </summary>
		/// <returns>
		///		The fully qualified path to the executable file.
		/// </returns>
		/*---------------------------------------------------------------------------------------------------------------*/
		protected override string GenerateFullPathToTool()
		{
			try
			{
				return Path.Combine(ToolPath, ToolName);
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
				return null;
			}
		}
		#endregion Public Methods
	}
}
