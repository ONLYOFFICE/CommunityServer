
// Copyright © 2006 Douglas Rohm

using System;
using Microsoft.Build.Framework;
using VaultClientNetLib;
using VaultClientOperationsLib;
using VaultLib;

namespace MSBuild.Community.Tasks.Vault
{
	/// <summary>
	/// Task that checks a file out of the repository.
	/// </summary>
	/// <example>
	/// <para></para>
	/// <code><![CDATA[
	/// <VaultCheckout Username="username" Password="password"
	///			Url="http://localhost" Repository="repository"
	///			Path="$/Temp/VaultTestFile.txt" Comment="Test for Vault Checkout task." 
	///			WorkingFolder="C:\Temp">
	///		<Output TaskParameter="Version" PropertyName="Version" />
	///	</VaultCheckout>
	///
	///	<Message Text="Version: $(Version)" />
	/// ]]></code>
	/// </example>
	public class VaultCheckout : VaultBase
	{
		#region Fields

		private string _repository = "";
		private string _comment = "";
		private string _fileName = "";
		
		#endregion

		#region Properties

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
		/// The file to check out from the specified repository.
		/// </summary>
		[Required]
		public string Path
		{
			get { return _fileName; }
			set { _fileName = value; }
		}

		/// <summary>
		/// Comment to attach to the files and/or folders being checked out.
		/// </summary>
		public string Comment
		{
			get { return _comment; }
			set { _comment = value; }
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
						Checkout(Path);
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
		private void Checkout(string fileName)
		{
			string normalizedPath = RepositoryPath.NormalizeFolder(fileName);

			if (IsVaultFolder(normalizedPath))
			{
				VaultClientFolder vaultClientFolder = ClientInstance.TreeCache.Repository.Root.FindFolderRecursive(normalizedPath);

				if (!String.IsNullOrEmpty(WorkingFolder))
				{
					ClientInstance.TreeCache.SetWorkingFolder(vaultClientFolder.FullPath, WorkingFolder);
				}
				
				ClientInstance.CheckOut(vaultClientFolder, true, VaultCheckOutType.CheckOut, Comment);
				ClientInstance.Get(vaultClientFolder, true, true, MakeWritableType.MakeAllFilesWritable, SetFileTimeType.Current, MergeType.OverwriteWorkingCopy, null);

				Version = Convert.ToInt32(vaultClientFolder.Version);
				Log.LogMessage(MessageImportance.Normal, string.Format(Properties.Resources.VaultCheckoutSuccessful), vaultClientFolder.Name);
			}

			if (IsVaultFile(normalizedPath))
			{
				VaultClientFile vaultClientFile = ClientInstance.TreeCache.Repository.Root.FindFileRecursive(normalizedPath);

				if (!String.IsNullOrEmpty(WorkingFolder))
				{
					ClientInstance.TreeCache.SetWorkingFolder(vaultClientFile.Parent.FullPath, WorkingFolder);
				}
				
				ClientInstance.CheckOut(vaultClientFile, VaultCheckOutType.CheckOut, Comment);
				ClientInstance.Get(vaultClientFile, true, MakeWritableType.MakeAllFilesWritable, SetFileTimeType.Current, MergeType.OverwriteWorkingCopy, null);

				Version = Convert.ToInt32(vaultClientFile.Version);
				Log.LogMessage(MessageImportance.Normal, string.Format(Properties.Resources.VaultCheckoutSuccessful, vaultClientFile.Name));
			}

			if (IsVaultFolder(normalizedPath) == false && IsVaultFile(normalizedPath) == false)
			{
				throw new Exception(string.Format(Properties.Resources.VaultResourceNotFound, normalizedPath));
			}
		}
		
		#endregion
	}
}
