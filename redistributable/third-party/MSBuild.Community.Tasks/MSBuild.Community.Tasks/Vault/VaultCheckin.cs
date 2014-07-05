
// Copyright © 2006 Douglas Rohm

using System;
using System.IO;
using Microsoft.Build.Framework;
using VaultClientNetLib;
using VaultClientOperationsLib;
using VaultLib;

namespace MSBuild.Community.Tasks.Vault
{
	/// <summary>
	/// Task that checks a file into the repository.
	/// </summary>
	/// <example>
	/// <para></para>
	/// <code><![CDATA[
	/// <VaultCheckin Username="username" Password="password"
	///			Url="http://localhost" Repository="repository"
	///			Path="$/Temp/VaultTestFile.txt" Comment="Test for Vault Checkin task." 
	///			WorkingFolder="C:\Temp" DiskFile="C:\Temp\VaultTestFile.txt">
	///		<Output TaskParameter="Version" PropertyName="Version" />
	///	</VaultCheckin>
	///	
	///	<Message Text="Version: $(Version)" />
	/// ]]></code>
	/// </example>
	public class VaultCheckin : VaultBase
	{
		#region Fields

		private string _repository = "";
		private string _comment = "";
		private string _fileName = "";
		private string _diskFile = "";
		
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
		/// The file to check in from the specified repository.
		/// </summary>
		[Required]
		public string Path
		{
			get { return _fileName; }
			set { _fileName = value; }
		}

		/// <summary>
		/// The path to a file on disk which will be checked in to the repository.  This is for
		/// use when you don't want to maintain a working folder for changes.
		/// </summary>
		public string DiskFile
		{
			get { return _diskFile; }
			set { _diskFile = value; }
		}

		/// <summary>
		/// Comment to attach to the files and/or folders being checked in.
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
						Checkin(Path);
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
		/// Checks the specified file or folder into the repository.
		/// </summary>
		/// <exception>Exception</exception>
		private void Checkin(string fileName)
		{
			string normalizedPath = RepositoryPath.NormalizeFolder(fileName);

			if (IsVaultFolder(normalizedPath))
			{
				VaultClientFolder vaultClientFolder = ClientInstance.TreeCache.Repository.Root.FindFolderRecursive(normalizedPath);

				if (!String.IsNullOrEmpty(WorkingFolder))
				{
					ClientInstance.TreeCache.SetWorkingFolder(vaultClientFolder.FullPath, WorkingFolder);
				}
				
				ClientInstance.Refresh();
				ChangeSetItemColl changeSet;
				BuildChangeSetOfCheckedOutFiles(vaultClientFolder, out changeSet);

				if (!ClientInstance.Commit(changeSet))
				{
					string errMsg = VaultConnection.GetSoapExceptionMessage(changeSet[0].Request.Response.Status);
					throw new Exception(string.Format(Properties.Resources.VaultCheckinFolderException, normalizedPath, errMsg));
				}
				else
				{
					Version = Convert.ToInt32(vaultClientFolder.Version);
					Log.LogMessage(MessageImportance.Normal, string.Format(Properties.Resources.VaultCheckinSuccessful), vaultClientFolder.Name);
				}
			}
			else if (IsVaultFile(normalizedPath))
			{
				string previousWorkingFolder = "";
				string repositoryFolderPath = "";
				string tmpdiskPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), string.Format("msbuild_checkinfor_{0}", System.IO.Path.GetFileName(normalizedPath)));

				VaultClientFile vaultClientFile = ClientInstance.TreeCache.Repository.Root.FindFileRecursive(normalizedPath);

				if (!String.IsNullOrEmpty(WorkingFolder))
				{
					ClientInstance.TreeCache.SetWorkingFolder(vaultClientFile.Parent.FullPath, WorkingFolder);
				}
				
				string diskFilename = System.IO.Path.Combine(ClientInstance.GetWorkingFolder(vaultClientFile).GetLocalFolderPath(),
					normalizedPath.Substring(normalizedPath.LastIndexOf(VaultDefine.PathSeparator) + 1));
				bool bChangeWorkingFolder = false;

				if (!Misc.stringIsBlank(_diskFile) && _diskFile != diskFilename)
				{
					bChangeWorkingFolder = true;
					if (!File.Exists(_diskFile))
					{
						throw new Exception(string.Format(Properties.Resources.VaultDiskFileDoesNotExist, _diskFile));
					}

					// They've specified a different disk file (no working folder)
					repositoryFolderPath = vaultClientFile.Parent.FullPath;
					previousWorkingFolder = ClientInstance.TreeCache.GetWorkingFolder(repositoryFolderPath);

					if (Directory.Exists(tmpdiskPath) == false)
					{
						Directory.CreateDirectory(tmpdiskPath);
					}

					// Set a different working folder to avoid interference with the real working folder
					ClientInstance.TreeCache.SetWorkingFolder(repositoryFolderPath, tmpdiskPath);
					diskFilename = System.IO.Path.Combine(tmpdiskPath, vaultClientFile.Name);
					Log.LogMessage(MessageImportance.Normal, string.Format(Properties.Resources.VaultSetNewWorkingFolder, repositoryFolderPath, tmpdiskPath, previousWorkingFolder));

					ClientInstance.CheckOut(vaultClientFile, 2, "Temp checkout for MSBuild Vault task.");
					ClientInstance.Get(vaultClientFile, true, MakeWritableType.MakeAllFilesWritable, SetFileTimeType.Current, MergeType.OverwriteWorkingCopy, null);
					if (File.Exists(diskFilename))
					{
						File.SetAttributes(diskFilename, FileAttributes.Normal);
						File.Delete(diskFilename);
					}
					File.Copy(_diskFile, diskFilename);
					ClientInstance.Refresh();
				}

				try
				{
					ChangeSetItemColl requestedChange = new ChangeSetItemColl();
					requestedChange.Add(new ChangeSetItem_Modified(DateTime.Now, _comment, "", vaultClientFile.ID, vaultClientFile.ObjVerID,
						diskFilename, normalizedPath, false, vaultClientFile.EOL));

					if (!ClientInstance.Commit(requestedChange))
					{
						string errMsg = VaultConnection.GetSoapExceptionMessage(requestedChange[0].Request.Response.Status);
						throw new Exception(string.Format(Properties.Resources.VaultCheckinFileException, normalizedPath, errMsg));
					}
					else
					{
						Version = Convert.ToInt32(vaultClientFile.Version);
						Log.LogMessage(MessageImportance.Normal, string.Format(Properties.Resources.VaultCheckinSuccessful, vaultClientFile.Name));
					}
				}
				finally
				{
					if (bChangeWorkingFolder)
					{
						if (Misc.stringIsBlank(previousWorkingFolder))
						{
							Log.LogMessage(MessageImportance.Normal, string.Format(Properties.Resources.VaultWorkingFolderCleared, repositoryFolderPath));
							ClientInstance.TreeCache.RemoveWorkingFolder(repositoryFolderPath);
						}
						else
						{
							Log.LogMessage(MessageImportance.Normal, string.Format(Properties.Resources.VaultWorkingFolderRestored, repositoryFolderPath, previousWorkingFolder));
							ClientInstance.TreeCache.SetWorkingFolder(repositoryFolderPath, previousWorkingFolder);
						}
					}
					if (Directory.Exists(tmpdiskPath))
					{
						Misc.DeleteDirectoryRecursivelyIncludingReadOnly(tmpdiskPath);
					}
				}
			}
			else
			{
				throw new Exception(string.Format(Properties.Resources.VaultCheckinFileNotFoundException, normalizedPath));
			}
		}

		private void BuildChangeSetOfCheckedOutFiles(VaultClientFolder folder, out ChangeSetItemColl changeSet)
		{
			if (ClientInstance.WorkingFolderOptions.RequireCheckOutBeforeCheckIn == false)
			{
				// Do a scan to update the change set list
				ClientInstance.UpdateKnownChanges_All(false);
			}

			// The new list of change set items
			changeSet = new ChangeSetItemColl();

			// Get the internal change set
			ChangeSetItemColl csic = ClientInstance.InternalChangeSet_GetItems(true);
			if ((csic != null) && (csic.Count > 0))
			{
				// From the full change list, build a new list including only those in the requested folder
				foreach (ChangeSetItem internalChange in csic)
				{
					if (internalChange.DisplayRepositoryPath.IndexOf(folder.FullPath) == 0)
					{
						changeSet.Add(internalChange);
					}
				}
			}
		}
		
		#endregion
	}
}
