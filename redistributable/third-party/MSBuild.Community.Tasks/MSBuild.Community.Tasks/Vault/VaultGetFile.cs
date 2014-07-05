
// Copyright © 2006 Douglas Rohm

using System;
using Microsoft.Build.Framework;
using VaultClientNetLib;
using VaultClientOperationsLib;

namespace MSBuild.Community.Tasks.Vault
{
	/// <summary>
	/// Task that gets a file from the repository.
	/// </summary>
	/// <example>
	/// <para></para>
	/// <code><![CDATA[
	/// <VaultGetFile username="username" Password="password"
	///			Url="http://localhost" Repository="repository" 
	///			Path="$/Temp/VaultTestFile.txt" WorkingFolder="C:\Temp">
	///		<Output TaskParameter="Version" PropertyName="Version" />
	/// </VaultGetFile>
	///
	///	<Message Text="Version: $(Version)" />
	/// ]]></code>
	/// </example>
	public class VaultGetFile : VaultBase
	{
		#region Fields

		private string _repository = "";
		private string _destination = "";
		private string _fileName = "";
		
		#endregion

		#region Properties

		/// <summary>
		/// Path to store retrieved files if no working folder is set.
		/// </summary>
		public string Destination
		{
			get { return _destination; }
			set { _destination = value; }
		}

		/// <summary>
		/// The Vault repository to be used.
		/// </summary>
		[Required]
		public string Repository
		{
			get { return _repository; }
			set { _repository = value; }
		}

		/// <summary>
		/// The file to retrieve from the specified repository.
		/// </summary>
		[Required]
		public string Path
		{
			get { return _fileName; }
			set { _fileName = value; }
		}
		
		#endregion

		/// <summary>
		/// Executes the task.
		/// </summary>
		/// <returns><see langword="true"/> if the task ran successfully; 
		/// otherwise <see langword="false"/>.</returns>
		public override bool Execute()
		{
			AccessLevel = VaultConnection.AccessLevelType.Client;
			bool bSuccess = true;

			try
			{
				Login();

				try
				{
					if (SelectRepository(Repository))
					{
						GetFile(Path);
					}
					else
					{
						Log.LogMessage(MessageImportance.High, string.Format(Properties.Resources.VaultRepositorySelectionFailure, Repository));
						bSuccess = false;
					}
				}
				catch (Exception ex)
				{
					Log.LogErrorFromException(ex);
					bSuccess = false;
				}
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
				bSuccess = false;
			}
			finally
			{
				Logout();
			}
			
			return bSuccess;
		}


		#region Private Helper Methods

		/// <summary>
		/// Retrieves the specified file.
		/// </summary>
		/// <exception>Exception</exception>
		private void GetFile(string fileName)
		{
			if (IsVaultFolder(fileName))
			{
				VaultClientFolder vaultClientFolder = ClientInstance.TreeCache.Repository.Root.FindFolderRecursive(fileName);

				if (Destination.Length == 0)
				{
					if (!String.IsNullOrEmpty(WorkingFolder))
					{
						ClientInstance.TreeCache.SetWorkingFolder(vaultClientFolder.FullPath, WorkingFolder);
					}
					
					ClientInstance.Get(vaultClientFolder, true, true, MakeWritableType.MakeAllFilesWritable, SetFileTimeType.Current, MergeType.OverwriteWorkingCopy, null);
				}
				else
				{
					ClientInstance.GetToNonWorkingFolder(vaultClientFolder, true, true, true, MakeWritableType.MakeAllFilesWritable, SetFileTimeType.Current, Destination, null);
				}
			}

			if (IsVaultFile(fileName))
			{
				VaultClientFile vaultClientFile = ClientInstance.TreeCache.Repository.Root.FindFileRecursive(fileName);

				if (Destination.Length == 0)
				{
					if (!String.IsNullOrEmpty(WorkingFolder))
					{
						ClientInstance.TreeCache.SetWorkingFolder(vaultClientFile.Parent.FullPath, WorkingFolder);
					}
					
					ClientInstance.Get(vaultClientFile, true, MakeWritableType.MakeAllFilesWritable, SetFileTimeType.Current, MergeType.OverwriteWorkingCopy, null);
				}
				else
				{
					ClientInstance.GetToNonWorkingFolder(vaultClientFile, true, true, MakeWritableType.MakeAllFilesWritable, SetFileTimeType.Current, vaultClientFile.Parent.FullPath, Destination, null);
				}
			}
		}
		
		#endregion
	}
}
