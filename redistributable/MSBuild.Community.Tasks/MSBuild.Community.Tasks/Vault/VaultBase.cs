
// Copyright © 2006 Douglas Rohm

using System;
using System.Globalization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using VaultClientNetLib;
using VaultClientOperationsLib;
using VaultLib;
using Resources=MSBuild.Community.Tasks.Properties.Resources;

namespace MSBuild.Community.Tasks.Vault
{
	/// <summary>
	/// Vault base abstract class.
	/// </summary>
	public abstract class VaultBase : Task
	{
		
		#region Fields

		// Basic connection variables required for most activities.
		private string _username = string.Empty;
		private string _password = string.Empty;
		private string _url = string.Empty;
		private string _workingFolder = string.Empty;
		private int _version = 0;

		private ClientInstance _clientInstance = new ClientInstance();
		private VaultConnection.AccessLevelType _accessLevel = VaultConnection.AccessLevelType.Client;
		private VaultRepositoryInfo _currentRepository = new VaultRepositoryInfo();
		
		#endregion

		#region Properties

		/// <summary>
		/// Accessor for the current access level:  Admin or Client.
		/// </summary>
		public VaultConnection.AccessLevelType AccessLevel
		{
			get { return _accessLevel; }
			set { _accessLevel = value; }
		}

		/// <summary>
		/// Url where the Vault installation is.  Required.
		/// </summary>
		/// <example>http://localhost</example>
		[Required]
		public string Url
		{
			get { return _url; }
			set { _url = value; }
		}

		/// <summary>
		/// The user name to use when logging into Vault.
		/// </summary>
		[Required]
		public string Username
		{
			get { return _username; }
			set { _username = value; }
		}

		/// <summary>
		/// The password to use in conjunction with the specified 
		/// username to log in to the Vault installation.
		/// </summary>
		public string Password
		{
			get { return _password; }
			set { _password = value; }
		}

		/// <summary>
		/// Accessor for _clientInstance.
		/// </summary>
		public ClientInstance ClientInstance
		{
			get { return _clientInstance; }
			set { _clientInstance = value; }
		}
		
		/// <summary>
		/// The working folder for the file or folder in the repository.
		/// </summary>
		public string WorkingFolder
		{
			get { return _workingFolder; }
			set { _workingFolder = value; }
		}
		
		/// <summary>
		/// Gets or sets the version number for the file or folder.
		/// </summary>
		[Output]
		public int Version
		{
			get { return _version; }
			set { _version = value; }
		}
		
		#endregion

		#region Protected Methods

		/// <summary>
		/// This function logs into the Vault server and chooses the appropriate repository.  It checks
		/// for a valid username, password, and successful login to the server.
		/// </summary>
		/// <exception>Exception</exception>
		public void Login()
		{
			if (Url.Length != 0 && Username.Length != 0)
			{
				ClientInstance.Init(_accessLevel);
				
				//Fixed Case to match properly. 
				if (Url.ToLower().EndsWith("/vaultservice") == false)
				{
					Url += "/VaultService";
				}
				if (Url.ToLower().StartsWith("http://") == false && Url.ToLower().StartsWith("https://") == false)
				{
					Url = "http://" + Url;
				}
				
				ClientInstance.Login(Url, Username, Password);

#if VAULT_3_5
				ClientInstance.EventEngine.addListener(this, typeof(StatusMessageEvent));
#else
				ClientInstance.StatusChanged += new StatusChangedEventHandler(StatusChangedHandler);
#endif

				if (ClientInstance.ConnectionStateType != ConnectionStateType.Connected)
				{
					throw new Exception(Resources.VaultLoginFailed);
				}
			}
			else
			{
				if (Url.Length == 0)
				{
					throw new Exception(Resources.VaultIncorrectParameters + "  " + Resources.VaultUrlRequired);
				}
				if (Username.Length == 0)
				{
					throw new Exception(Resources.VaultIncorrectParameters + "  " + Resources.VaultUsernameRequired);
				}
			}
		}

		/// <summary>
		/// Used to logout from the Vault Server.
		/// </summary>
		public void Logout()
		{
			if (ClientInstance != null)
			{
				ClientInstance.Logout();
			}
		}
		
		/// <summary>
		/// This function chooses the active repository on the server.
		/// </summary>
		/// <param name="repositoryName">Name of the repository to select.</param>
		/// <returns>True if the repository selection succeeded, false otherwise.</returns>
		public bool SelectRepository(string repositoryName)
		{
			if (ClientInstance.ConnectionStateType != ConnectionStateType.Connected)
			{
				return false;
			}

			VaultRepositoryInfo[] vaultRepositories = null;
			ClientInstance.ListRepositories(ref vaultRepositories);

			foreach (VaultRepositoryInfo vaultRepositoryInfo in vaultRepositories)
			{
				//Beter way to ignore case.
				if (string.Compare(vaultRepositoryInfo.RepName, repositoryName, true, CultureInfo.CurrentCulture) == 0)
				{
					_currentRepository = vaultRepositoryInfo;
					ClientInstance.SetActiveRepositoryID(_currentRepository.RepID, Username, _currentRepository.UniqueRepID, true, true);

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Helper function used to simplify many of the tasks.  It simply checks to see if the supplied
		/// string is a valid Vault folder or not.
		/// </summary>
		/// <param name="folderName">Name of the folder you wish to check for the existence of.</param>
		/// <returns>True if a folder exists, false otherwise.</returns>
		public bool IsVaultFolder(string folderName)
		{
			VaultClientFolder vaultClientFolder;

			string normalizedPath = RepositoryPath.NormalizeFolder(folderName);
			if (ClientInstance.TreeCache != null)
			{
				vaultClientFolder = ClientInstance.TreeCache.Repository.Root.FindFolderRecursive(normalizedPath);
			}
			else
			{
				throw new Exception(Resources.VaultTreeCacheFailure);
			}
			if (vaultClientFolder == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Help function used to simplify many of the tasks.  It simply checks to see if the supplied
		/// string is a valid Vault file or not.
		/// </summary>
		/// <param name="fileName">Name of the file you wish to check for the existence of.</param>
		/// <returns>True if the file exists, false otherwise.</returns>
		public bool IsVaultFile(string fileName)
		{
			VaultClientFile vaultClientFile;

			string normalizedPath = RepositoryPath.NormalizeFolder(fileName);
			vaultClientFile = ClientInstance.TreeCache.Repository.Root.FindFileRecursive(normalizedPath);

			if (vaultClientFile == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// This function is used to interact with the logging facility of NAnt.
		/// </summary>
		/// <param name="logData">String to be passed to NAnt.</param>
		public void VaultLog(string logData)
		{
			Log.LogMessage(MessageImportance.Normal, logData);
		}
		
		/// <summary>
		/// This function is used to interact with the logging facility of NAnt.
		/// </summary>
		/// <param name="logData">String to be passed to NAnt.</param>
		public void VaultLogVerbose(string logData)
		{
			Log.LogMessage(MessageImportance.Normal, logData);
		}

#if VAULT_3_5
		private void HandleEvent(StatusMessageEvent e)
		{
			Log.LogMessage(MessageImportance.Normal, "Vault: " + e.Message);
		}
#else
		private void StatusChangedHandler(object sender, string message)
		{
			Log.LogMessage(MessageImportance.Normal, "Vault: " + message);
		}
#endif
		
		#endregion
	}
}
