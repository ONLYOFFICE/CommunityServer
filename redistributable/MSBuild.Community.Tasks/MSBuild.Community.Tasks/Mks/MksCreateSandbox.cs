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
	///		Custom MSBuild task that calls the MKS Create Sandbox command.
	/// </summary>
	/// <example>Creates a new sandbox on the client machine in a specified directory.
	/// <code><![CDATA[
	/// <Target Name="CreateSandbox">
	///		<MksCreateSandbox 
	///			Directory="$(SandboxDir)"
	///			Populate="true"
	///			Project="$(MksProject)" 
	///			Recurse="true"		
	///		/>
	///	</Target>
	/// ]]></code>
	/// </example>
	/*---------------------------------------------------------------------------------------------------------------*/
	public class MksCreateSandbox : ToolTask
	{
		#region Constants
		const string COMMAND = "createsandbox";
		#endregion Constants

		#region Private Member Variables
		private string _directory;
		private bool   _populate;
		private string _project;
		private bool   _recurse;
		#endregion Private Member Variables

		#region Contructors
		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Contructor that initializes ToolTask properties.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		public MksCreateSandbox()
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
		///		Returns the text for the MKS Create Sandbox CLI command.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		public string CommandText
		{
			get { return GenerateFullPathToTool() + " " + GenerateCommandLineCommands(); }
		}
		
		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Identify a location on the MKS Source Integrity client system for the new sandbox. Use spaces to identify 
		///		more than one directory.
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
		///		Control whether to populate the sandbox with read-only working files for all members.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		[Required]
		public bool Populate
		{
			get { return _populate; }
			set { _populate = value; }
		}

		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Specifies the location of a project.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		[Required]
		public string Project
		{
			get { return _project; }
			set { _project = value; }
		}

		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Specificy whether to recursively create sanboxes for any subprojects.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		[Required]
		public bool Recurse
		{
			get { return _recurse; }
			set { _recurse = value; }
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
				if (string.IsNullOrEmpty(_directory)
				||	string.IsNullOrEmpty(_project))
				{
					throw new ArgumentNullException();
				}

				// Build the CLI command to execute an MKS Create Sandbox
				stringBuilder.AppendFormat(COMMAND);
				
				if (_populate)
				{
					stringBuilder.AppendFormat(" --populate");
				}
				else
				{
					stringBuilder.AppendFormat(" --nopopulate");
				}

				if (_recurse)
				{
					stringBuilder.AppendFormat(" --recurse");
				}
				else
				{
					stringBuilder.AppendFormat(" --norecurse");
				}

				stringBuilder.AppendFormat(" --project=" + _project);
				stringBuilder.AppendFormat(" " + _directory);

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
