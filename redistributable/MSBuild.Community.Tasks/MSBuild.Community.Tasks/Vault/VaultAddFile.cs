
// Copyright © 2006 Douglas Rohm

using System;
using System.IO;
using Microsoft.Build.Framework;
using VaultClientNetLib;
using VaultClientOperationsLib;

namespace MSBuild.Community.Tasks.Vault
{
	/// <summary>
	/// Adds a file or folder from the local disk to a Vault repository.
	/// </summary>
	/// <example>
	/// <para></para>
	/// <code><![CDATA[
	/// <VaultAddFile username="username" Password="password"
	///			Url="http://localhost" Repository="repository" 
	///			Path="$/Temp/VaultTestFile.txt" WorkingFolder="C:\Temp">
	///		<Output TaskParameter="Version" PropertyName="Version" />
	/// </VaultAddFile>
	///
	///	<Message Text="Version: $(Version)" />
	/// ]]></code>
	/// </example>
	public class VaultAddFile : VaultBase
	{
		#region Fields

		private string _repository = "";
		private ITaskItem[] _addFileSet;
		private string _repositoryPath = "";
		private string _comment = "";
		private VaultClientFolder Folder = null;
		private ChangeSetItemColl _changeSetCollection = new ChangeSetItemColl();
		
		#endregion

		#region Properties

		/// <summary>
		/// The name of the Vault repository to be used.
		/// </summary>
		[Required]
		public string Repository
		{
			get { return _repository; }
			set { _repository = value; }
		}

		/// <summary>
		/// The file(s) to add to the specified repository.
		/// </summary>
		public ITaskItem[] AddFileSet
		{
			get { return _addFileSet; }
			set { _addFileSet = value; }
		}

		/// <summary>
		/// Comment to attach to the files and/or folders being added.
		/// </summary>
		public string Comment
		{
			get { return _comment; }
			set { _comment = value; }
		}

		/// <summary>
		/// Root path in the repository where the file(s) will be added.
		/// </summary>
		[Required]
		public string Path
		{
			get { return _repositoryPath; }
			set { _repositoryPath = value; }
		}
		
		#endregion

		/// <summary>
		/// Executes the task.
		/// </summary>
		/// <returns><see langword="true"/> if the task ran successfully; 
		/// otherwise <see langword="false"/>.</returns>
		public override bool Execute()
		{
			AccessLevel = VaultConnection.AccessLevelType.Admin;
			bool bSuccess = true;

			try
			{
				Login();

				try
				{
					if (SelectRepository(Repository))
					{
						FindFolder();

						foreach (ITaskItem item in _addFileSet)
						{
							AddFileToCollection(item.ItemSpec);
						}
						
						if (ClientInstance.Commit(_changeSetCollection))
						{
							Log.LogMessage(MessageImportance.Normal, Properties.Resources.VaultAddFileCommitSucceeded);
						}
						else
						{
							throw new Exception(Properties.Resources.VaultAddFileCommitFailed);
						}
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
		/// Add a file to the change set collection.
		/// </summary>
		/// <param name="fileName">Full path and filename of the file to add.</param>
		/// <exception>Exception</exception>
		private void AddFileToCollection(string fileName)
		{
			string repositoryPath = Folder.FullPath + "/" + System.IO.Path.GetFileName(fileName);

			if (File.Exists(fileName))
			{
				ChangeSetItem_AddFile changeSetItem = new ChangeSetItem_AddFile(DateTime.Now, Comment, String.Empty, fileName, repositoryPath);
				_changeSetCollection.Add(changeSetItem);
				Log.LogMessage(MessageImportance.Normal, string.Format(Properties.Resources.VaultFileAddedToChangeSet, fileName));
			}
			else if (Directory.Exists(fileName))
			{
				ChangeSetItem_AddFolder changeSetItem = new ChangeSetItem_AddFolder(DateTime.Now, Comment, String.Empty, fileName, repositoryPath);
				_changeSetCollection.Add(changeSetItem);
				Log.LogMessage(MessageImportance.Normal, string.Format(Properties.Resources.VaultFolderAddedToChangeSet, fileName));
			}
			else
			{
				throw new Exception(Properties.Resources.VaultAddFilesException);
			}
		}

		/// <summary>
		/// Attempts to locate the Vault folder using the directory path specified.
		/// </summary>
		/// <exception>Exception</exception>
		private void FindFolder()
		{
			Path = VaultLib.RepositoryPath.NormalizeFolder(Path);
			Folder = ClientInstance.TreeCache.Repository.Root.FindFolderRecursive(Path);

			if (Folder == null)
			{
				throw new Exception(Properties.Resources.VaultPathValidationException);
			}
		}
		
		#endregion
	}
}
