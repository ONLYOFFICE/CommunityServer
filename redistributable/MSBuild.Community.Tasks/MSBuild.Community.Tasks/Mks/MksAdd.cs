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
	///		Custom MSBuild task that calls the MKS Add command.
	/// </summary>
	/// <example>Add one or more nonmember files located in a sandbox directory to a project.
	/// <code><![CDATA[
	/// <Target Name="Add">
	///		<MksAdd 
	///			Cpid=":none"
	///			CreateSubprojects="true"
	///			Description = "'Add MksTest.cs class file.'"
	///			Directory="$(SandboxDir)"
	///			Exclude="file:*.pj" 
	///			NonMembers="$(SandboxDir)\MksTest.cs"
	///			OnExistingArchive="newarchive"
	///			Recurse="false"		
	///		/>
	/// </Target>
	/// ]]></code>
	/// </example>
	/*---------------------------------------------------------------------------------------------------------------*/
	public class MksAdd : ToolTask
	{
		#region Constants
		const string COMMAND = "add";
		#endregion Constants

		#region Private Member Variables
		private string _cpid;
		private bool   _createSubprojects;
		private string _description;
		private string _directory;
		private string _exclude;
		private string _include;
		private string _nonMembers;
		private string _onExistingArchive;
		private bool   _recurse;
		#endregion Private Member Variables

		#region Contructors
		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Contructor that initializes ToolTask properties.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		public MksAdd()
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
		///		Returns the text for the MKS Add CLI command.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		public string CommandText
		{
			get { return GenerateFullPathToTool() + " " + GenerateCommandLineCommands(); }
		}

		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		The name of the identifying change package that is notified of this action.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		[Required]
		public string Cpid
		{
			get { return _cpid; }
			set { _cpid = value; }
		}

		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Control whether to create subprojects for each subdirectory encountered when adding members.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		[Required]
		public bool CreateSubprojects
		{
			get { return _createSubprojects; }
			set { _createSubprojects = value; }
		}

		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		The description of the changes associated with the members to Add.
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
		///		The directory for Add to be executed in.  Any files and members in the selection are treated as being
		///		relative to that directory.
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
		///		Specify one or more files to exclude when adding members.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		public string Exclude
		{
			get { return _exclude; }
			set { _exclude = value; }
		}

		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Specify one or more files to include when adding members.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		public string Include
		{
			get { return _include; }
			set { _include = value; }
		}

		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Identifies a specific file to add to your sandbox; use spaces to specify more than one.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		[Required]
		public string NonMembers
		{
			get { return _nonMembers; }
			set { _nonMembers = value; }
		}
		
		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Control whether to allow sharing of this member's history between projects, or to create a new archive if
		///		there is already an existing archive for the member.
		/// </summary>
		/*---------------------------------------------------------------------------------------------------------------*/
		[Required]
		public string OnExistingArchive
		{
			get { return _onExistingArchive; }
			set { _onExistingArchive = value; }
		}

		/*---------------------------------------------------------------------------------------------------------------*/
		/// <summary>
		///		Specificy whether to recursively add any subprojects and members.
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
				if (string.IsNullOrEmpty(_cpid)
				||  string.IsNullOrEmpty(_description)
				||  string.IsNullOrEmpty(_directory)
				||  string.IsNullOrEmpty(_nonMembers)
				||  string.IsNullOrEmpty(_onExistingArchive))
				{
					throw new ArgumentNullException();
				}

				// Build the CLI command to execute an MKS Add
				stringBuilder.AppendFormat(COMMAND);
				stringBuilder.AppendFormat(" --cpid={0}", _cpid);
				stringBuilder.AppendFormat(" --description={0}", _description);
				stringBuilder.AppendFormat(" --cwd={0}", _directory);
				stringBuilder.AppendFormat(" --onExistingArchive={0}", _onExistingArchive);

				if (!string.IsNullOrEmpty(_exclude))
				{
					stringBuilder.AppendFormat(" --exclude={0}", _exclude);
				}

				if (!string.IsNullOrEmpty(_include))
				{
					stringBuilder.AppendFormat(" --include={0}", _include);
				}

				if (_createSubprojects)
				{
					stringBuilder.AppendFormat(" --createSubprojects");
				}
				else
				{
					stringBuilder.AppendFormat(" --nocreateSubprojects");
				}
				
				if (_recurse)
				{
					stringBuilder.AppendFormat(" --recurse");
				}
				else
				{
					stringBuilder.AppendFormat(" --norecurse");
				}

				stringBuilder.AppendFormat(" {0}", _nonMembers);

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
