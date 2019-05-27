#region Copyright � 2005 Paul Welter. All rights reserved.
/*
Copyright � 2005 Paul Welter. All rights reserved.

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
using System.Management;

namespace MSBuild.Community.Tasks.IIS
{
	/// <summary>
	/// Base task for any IIS-related task.
	/// </summary>
	/// <remarks>Stores the base logic for gathering the IIS version and server and port checking.  This
	/// base task also stores common properties for other related tasks.</remarks>
	public abstract class WebBase : Task
	{
		/// <summary>
		/// Defines the possible IIS versions supported by the task.
		/// </summary>
		protected enum IISVersion
		{
			NotSupported,
			/// <summary>
			/// IIS version 4.x
			/// </summary>
			Four,
			/// <summary>
			/// IIS version 5.x
			/// </summary>
			Five,
			/// <summary>
			/// IIS version 6.x
			/// </summary>
			Six,
			/// <summary>
			/// IIS version 7.0
			/// </summary>
			Seven,
			/// <summary>
			/// IIS version 7.5
			/// </summary>
			SevenFive,
			/// <summary>
			/// IIS version 8.0
			/// </summary>
			Eight,
			/// <summary>
			/// IIS version 8.5
			/// </summary>
			EightFive
		}

		/// <summary>
		/// Defines the possible application pool actions to be performed.
		/// </summary>
		public enum ApplicationPoolAction
		{
			/// <summary>
			/// Recycles an application pool.
			/// </summary>
			Recycle = 1,

			/// <summary>
			/// Stops and restarts the application pool.
			/// </summary>
			Restart = 2,

			/// <summary>
			/// Starts the application pool.
			/// </summary>
			Start = 3,

			/// <summary>
			/// Stops the application pool.
			/// </summary>
			Stop = 4
		}

		/// <summary>
		/// Defines the current application pool state.
		/// </summary>
		protected enum IIS7ApplicationPoolState
		{
			/// <summary>
			/// The application pool is starting.
			/// </summary>
			Starting = 0,

			/// <summary>
			/// The application pool has started.
			/// </summary>
			Started = 1,

			/// <summary>
			/// The application pool is stopping.
			/// </summary>
			Stopping = 2,

			/// <summary>
			/// The application pool has stopped.
			/// </summary>
			Stopped = 3,

			/// <summary>
			/// The application pool is in unknown state.
			/// </summary>
			Unknown = 4
		}

		#region Fields

		/// <summary>
		/// IIS version.
		/// </summary>
		protected IISVersion mIISVersion;
		private string mServerName = "localhost";
		private int mServerPort = 80;
		private string mServerInstance;
		private string mIISServerPath;
		private string mIISSiteHostHeaderName;
		private string mIISApplicationPath;
		private string mIISAppPoolPath;
		private string mUsername;
		private string mPassword;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the server.  The default value is 'localhost'.
		/// </summary>
		/// <value>The name of the server.</value>
		public string ServerName
		{
			get
			{
				return mServerName;
			}
			set
			{
				mServerName = value;
			}
		}

		/// <summary>
		/// Gets or sets host header. Used when you have more than one website in IIS that have the same port but different host headers.
		/// </summary>
		/// <value>The name of the host header.</value>
		public string HostHeaderName
		{
			get
			{
				return mIISSiteHostHeaderName;
			}
			set
			{
				mIISSiteHostHeaderName = value;
			}
		}

		/// <summary>
		/// Gets or sets the server port.
		/// </summary>
		/// <value>The server port.</value>
		public int ServerPort
		{
			get
			{
				return mServerPort;
			}
			set
			{
				mServerPort = value;
			}
		}

		/// <summary>
		/// Gets or sets the IIS server path.
		/// </summary>
		/// <remarks>Is in the form 'IIS://localhost/W3SVC/1/Root'.</remarks>
		/// <value>The IIS server path.</value>
		protected string IISServerPath
		{
			get
			{
				return mIISServerPath;
			}
			set
			{
				mIISServerPath = value;
			}
		}

		/// <summary>
		/// Gets or sets the application path.
		/// </summary>
		/// <remarks>Is in the form '/LM/W3SVC/1/Root'.</remarks>
		/// <value>The application path.</value>
		protected string IISApplicationPath
		{
			get
			{
				return mIISApplicationPath;
			}
			set
			{
				mIISApplicationPath = value;
			}
		}

		/// <summary>
		/// Gets or sets the IIS application pool path.
		/// </summary>
		/// <remarks>Is in the form 'IIS://localhost/W3SVC/AppPools'.</remarks>
		/// <value>The IIS application pool path.</value>
		protected string IISAppPoolPath
		{
			get
			{
				return mIISAppPoolPath;
			}
			set
			{
				mIISAppPoolPath = value;
			}
		}

		/// <summary>
		/// Gets or sets the username for the account the task will run under.  This property
		/// is needed if you specified a <see cref="ServerName"/> for a remote machine.
		/// </summary>
		/// <value>The username of the account.</value>
		public string Username
		{
			get
			{
				return mUsername;
			}
			set
			{
				mUsername = value;
			}
		}

		/// <summary>
		/// Gets or sets the password for the account the task will run under.  This property
		/// is needed if you specified a <see cref="ServerName"/> for a remote machine.
		/// </summary>
		/// <value>The password of the account.</value>
		public string Password
		{
			get
			{
				return mPassword;
			}
			set
			{
				mPassword = value;
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Gets the IIS version.
		/// </summary>
		/// <returns>The <see cref="IISVersion"/> for IIS.</returns>
		/// <exclude/>
		protected IISVersion GetIISVersion()
		{
			System.Version osVersion;

			if (mServerName == "localhost")
			{
				osVersion = Environment.OSVersion.Version;
			}
			else
			{
				// Make call to remote machine for OS version
				osVersion = GetRemoteOSVersion();
			}

			IISVersion iisVersion = IISVersion.Six;

			if (osVersion.Major < 5)
			{
				// Windows NT: IIS 4
				iisVersion = IISVersion.Four;
			}
			else if( osVersion.Major == 5)
			{
				switch (osVersion.Minor)
				{
					case 0:
					case 1:
						// Windows 2000 or Windows XP: IIS 5
						iisVersion = IISVersion.Five;
						break;
					case 2:
						// Windows Server 2003: IIS 6
						iisVersion = IISVersion.Six;
						break;
				}
			}
			else if (osVersion.Major == 6)
			{
				switch (osVersion.Minor)
				{
					case 0:
						// Windows Vista and Windows Server 2008
						iisVersion = IISVersion.Seven;
						break;
					case 1:
						// Windows 7 and Windows 2008 R2
						iisVersion = IISVersion.SevenFive;
						break;
					case 2:
						// Windows 8 and Windows Server 2012
						iisVersion = IISVersion.Eight;
						break;
					case 3:
						// Windows 8.1 and Windows Server 2012 R2
						iisVersion = IISVersion.EightFive;
						break;
				}
			}

			return iisVersion;
		}

		/// <summary>
		/// Gets the remote machine OS version.
		/// </summary>
		/// <returns>Returns a <see cref="System.Version"/> of the operating system.</returns>
		/// <exclude/>
		protected System.Version GetRemoteOSVersion()
		{
			System.Version remoteOSVersion = new System.Version();
			ConnectionOptions co = new ConnectionOptions();

			// In order to get operating system version on a remote machine, the task must
			// be executing with Administrative privaledges.
			if (Username != null && Password != null)
			{
				co.Username = Username;
				co.Password = Password;
			}

			ManagementScope ms = new ManagementScope("\\\\" + mServerName + "\\root\\cimv2", co);
			ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
			ManagementObjectSearcher query = new ManagementObjectSearcher(ms, oq);

			try
			{
				ManagementObjectCollection queryCollection = query.Get();
				foreach (ManagementObject mo in queryCollection)
				{
					remoteOSVersion = new System.Version(mo["Version"].ToString());
				}
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
			}

			return remoteOSVersion;
		}

		/// <summary>
		/// Verifies that the IIS root exists based on the <see cref="ServerName"/> and <see cref="ServerPort"/>.
		/// </summary>
		/// <exclude/>
		protected void VerifyIISRoot()
		{
			bool iisExists = false;

			DirectoryEntry iisRoot = new DirectoryEntry(string.Format("IIS://{0}/W3SVC", mServerName));
			iisRoot.RefreshCache();

			// Find specified server and port in collection
			foreach (DirectoryEntry site in iisRoot.Children)
			{
				if (site.SchemaClassName == "IIsWebServer")
				{
					if (VerifySiteHostHeaderExists(site))
					{
						if (VerifyServerPortExists(site))
						{
							iisExists = true;
						}
					}
				}

				site.Close();
			}

			iisRoot.Close();

			if (!iisExists)
			{
				string errorMessage = string.Format("Server '{0}:{1}' does not exist or is not reachable", mServerName, mServerPort);
				if (HostHeaderName != null)
				{
					errorMessage += string.Format(" with specified host header of '{0}'", HostHeaderName);
				}
				errorMessage += string.Format(".");

				throw new Exception(errorMessage);
			}
		}

		/// <summary>
		/// Verify that the IIS Website exists if it has been specified.
		/// </summary>
		/// <param name="site">DirectoryEntry that meets the IISWebServer schema</param>
		/// <returns>True if a site is found when specified. True if no site has been specified.</returns>
		private bool VerifySiteHostHeaderExists(DirectoryEntry site)
		{
			bool siteHeaderFound = false;

			if (HostHeaderName == null)
			{
				siteHeaderFound = true;
			}
			else
			{
				//get the serverBinding information
				foreach (object serverBinding in (PropertyValueCollection)site.Properties["ServerBindings"])
				{
					//find the IIS Website that has been specified by the user
					if (((string)serverBinding).ToLower().IndexOf(HostHeaderName.ToLower()) != -1)
					{
						siteHeaderFound = true;
						break;
					}
				}
			}

			return siteHeaderFound;
		}

		/// <summary>
		/// Helper method for <see cref="VerifyIISRoot"/> that verifies the server port exists.
		/// </summary>
		/// <param name="site">The site to verify the port.</param>
		/// <returns>Boolean value indicating the status of the port check.</returns>
		/// <exclude/>
		private bool VerifyServerPortExists(DirectoryEntry site)
		{
			bool portServerExists = false;

			//get the serverBinding information
			foreach (object serverBinding in (PropertyValueCollection)site.Properties["ServerBindings"])
			{

				//look for an integer
				foreach (string bindingPiece in ((string)serverBinding).Split(':'))
				{
					int serverPort;
					int.TryParse(bindingPiece, out serverPort);
					if (serverPort != 0)
					{
						if (ServerPort == serverPort)
						{
							SetIISPrimitives(site.Name);
							portServerExists = true;
							break;
						}
					}
				}
				if (portServerExists)
				{
					break;
				}
			}

			return portServerExists;
		}

		/// <summary>
		/// Sets some of the protected properties for the Virtual Directory Creation Wizard.
		/// </summary>
		/// <param name="serverInstance">DirectoryEntry.Name where the Entry is an IISWebServer schema</param>
		private void SetIISPrimitives(string serverInstance)
		{
			mServerInstance = serverInstance;
			mIISServerPath = string.Format("IIS://{0}/W3SVC/{1}/Root", mServerName, mServerInstance);
			mIISApplicationPath = string.Format("/LM/W3SVC/{0}/Root", mServerInstance);
			mIISAppPoolPath = string.Format("IIS://{0}/W3SVC/AppPools", mServerName);
		}

		#endregion
	}
}
