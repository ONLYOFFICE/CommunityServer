
// Copyright © 2006 Douglas Rohm

using System;
using Microsoft.Build.Framework;
using VaultClientNetLib;
using VaultClientOperationsLib;
using VaultLib;

namespace MSBuild.Community.Tasks.Vault
{
	/// <summary>
	/// Task that undo's a checkout of a file from the repository.
	/// </summary>
	/// <example>
	/// <para></para>
	/// <code><![CDATA[
	/// <VaultUndoCheckout username="username" Password="password"
	///			Url="http://localhost" Repository="repository" 
	///			Path="$/Temp/VaultTestFile.txt" Comment="Test for Vault UndoCheckout task."
	///			WorkingFolder="C:\Temp">
	///		<Output TaskParameter="Version" PropertyName="Version" />
	/// </VaultUndoCheckout>
	///
	///	<Message Text="Version: $(Version)" />
	/// ]]></code>
	/// </example>
	public class VaultUndoCheckout : VaultBase
	{
		#region Fields

		private string _repository = "";
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
		/// The file to undo checkout.
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
						UndoCheckout(Path);
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
		/// Undo checkout on the specified file.
		/// </summary>
		/// <exception>Exception</exception>
		private void UndoCheckout(string fileName)
		{
			string normalizedPath = RepositoryPath.NormalizeFolder(fileName);

			if (IsVaultFolder(normalizedPath))
			{
				VaultClientFolder vaultClientFolder = ClientInstance.TreeCache.Repository.Root.FindFolderRecursive(normalizedPath);

				if (!String.IsNullOrEmpty(WorkingFolder))
				{
					ClientInstance.TreeCache.SetWorkingFolder(vaultClientFolder.FullPath, WorkingFolder);
				}
				
				ClientInstance.UndoCheckOut(vaultClientFolder, true, LocalCopyType.Replace);
				Log.LogMessage(MessageImportance.Normal, string.Format(Properties.Resources.VaultUndoCheckoutSuccessful, vaultClientFolder.Name));
			}

			if (IsVaultFile(normalizedPath))
			{
				VaultClientFile vaultClientFile = ClientInstance.TreeCache.Repository.Root.FindFileRecursive(normalizedPath);

				if (!String.IsNullOrEmpty(WorkingFolder))
				{
					ClientInstance.TreeCache.SetWorkingFolder(vaultClientFile.Parent.FullPath, WorkingFolder);
				}
				
				ClientInstance.UndoCheckOut(vaultClientFile, LocalCopyType.Replace);
				Log.LogMessage(MessageImportance.Normal, string.Format(Properties.Resources.VaultUndoCheckoutSuccessful, vaultClientFile.Name));
			}

			if (IsVaultFolder(normalizedPath) == false && IsVaultFile(normalizedPath) == false)
			{
				throw new Exception(string.Format(Properties.Resources.VaultResourceNotFound, normalizedPath));
			}
		}
		
		#endregion
	}
}
