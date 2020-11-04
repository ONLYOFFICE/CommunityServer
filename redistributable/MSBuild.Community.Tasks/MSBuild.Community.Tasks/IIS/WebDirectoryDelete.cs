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
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.DirectoryServices;

namespace MSBuild.Community.Tasks.IIS
{
	/// <summary>
	/// Deletes a web directory on a local or remote machine with IIS installed.  The default is 
	/// to delete the web directory on the local machine.  If connecting to a remote machine, you
	/// can specify the <see cref="WebBase.Username"/> and <see cref="WebBase.Password"/> for the
	/// task to run under.
	/// </summary>
	/// <example>Deletes a web directory on the local machine.
	/// <code><![CDATA[
	/// <WebDirectoryDelete VirtualDirectoryName="MyVirDir" />
	/// ]]></code>
	/// </example>
	public class WebDirectoryDelete : WebBase
	{
		#region Fields

		private string mVirtualDirectoryName;

		#endregion

		#region Properties

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

		#endregion

		/// <summary>
		/// When overridden in a derived class, executes the task.
		/// </summary>
		/// <returns>
		/// True if the task successfully executed; otherwise, false.
		/// </returns>
        public override bool Execute()
        {
			if (DeleteVirtualDirectory())
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		#region Private Methods

		private bool DeleteVirtualDirectory()
		{
			bool bSuccess = false;

			try
			{
				Log.LogMessage(MessageImportance.Normal, "Deleting virtual directory '{0}' on '{1}:{2}'.", VirtualDirectoryName, ServerName, ServerPort);

				VerifyIISRoot();

				DirectoryEntry iisRoot = new DirectoryEntry(IISServerPath);
				DirectoryEntries childEntries = iisRoot.Children;
				iisRoot.RefreshCache();
				
				DirectoryEntry childVDir = iisRoot.Children.Find(VirtualDirectoryName, iisRoot.SchemaClassName);
				childEntries.Remove(childVDir);

				childVDir.Close();
				iisRoot.Close();

				bSuccess = true;
				Log.LogMessage(MessageImportance.Normal, "Done.");
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
			}

			return bSuccess;
		}

		#endregion
	}
}
