/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ASC.Files.Core;
using ASC.Migration.Core.Models;
using ASC.Migration.Resources;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.WCFService;

using ASCFile = ASC.Files.Core.File;
using ASCShare = ASC.Files.Core.Security.FileShare;
using File = System.IO.File;

namespace ASC.Migration.OwnCloud.Models
{
    public class OCMigratingFiles : MigratingFiles
    {
        public override int FoldersCount => foldersCount;

        public override int FilesCount => filesCount;

        public override long BytesTotal => bytesTotal;

        public override string ModuleName => MigrationResource.NextcloudModuleNameDocuments;

        private OCMigratingUser user;
        private string rootFolder;
        private List<OCFileCache> files;
        private List<OCFileCache> folders;
        private int foldersCount;
        private int filesCount;
        private long bytesTotal;
        private OCStorages storages;
        private Dictionary<string, OCMigratingUser> users;
        private Dictionary<string, OCMigratingGroups> groups;
        private Dictionary<object, int> matchingFileId;
        private string folderCreation;
        public OCMigratingFiles(OCMigratingUser user, OCStorages storages, string rootFolder, Action<string, Exception> log) : base(log)
        {
            this.user = user;
            this.rootFolder = rootFolder;
            this.storages = storages;
        }

        public override void Parse()
        {
            var drivePath = Directory.Exists(Path.Combine(rootFolder, "data", user.Key, "files")) ?
                Path.Combine(rootFolder, "data", user.Key, "files") : null;
            if (drivePath == null) return;

            files = new List<OCFileCache>();
            folders = new List<OCFileCache>();
            folderCreation = folderCreation != null ? folderCreation : DateTime.Now.ToString("dd.MM.yyyy");
            foreach (var entry in storages.FileCache)
            {
                string[] paths = entry.Path.Split('/');
                if (paths[0] != "files") continue;

                paths[0] = "OwnCloud’s Files " + folderCreation;
                entry.Path = string.Join("/", paths);

                if (paths.Length >= 1)
                {
                    string tmpPath = drivePath;
                    for (var i = 1; i < paths.Length; i++)
                    {
                        tmpPath = Path.Combine(tmpPath, paths[i]);
                    }
                    if (Directory.Exists(tmpPath) || File.Exists(tmpPath))
                    {
                        var attr = File.GetAttributes(tmpPath);
                        if (attr.HasFlag(FileAttributes.Directory))
                        {
                            foldersCount++;
                            folders.Add(entry);
                        }
                        else
                        {
                            filesCount++;
                            var fi = new FileInfo(tmpPath);
                            bytesTotal += fi.Length;
                            files.Add(entry);
                        }
                    }
                }
            }
        }

        public override void Migrate()
        {

            if (!ShouldImport) return;

            var drivePath = Directory.Exists(Path.Combine(rootFolder, "data", user.Key, "files")) ?
                Path.Combine(rootFolder, "data", user.Key) : null;
            if (drivePath == null) return;
            matchingFileId = new Dictionary<object, int>();
            var foldersDict = new Dictionary<string, Folder>();
            if (folders != null)
            {
                foreach (var folder in folders)
                {
                    var split = folder.Path.Split('/');
                    for (var i = 0; i < split.Length; i++)
                    {
                        var path = string.Join(Path.DirectorySeparatorChar.ToString(), split.Take(i + 1));
                        if (foldersDict.ContainsKey(path)) continue;
                        var parentId = i == 0 ? Global.FolderMy : foldersDict[string.Join(Path.DirectorySeparatorChar.ToString(), split.Take(i))].ID;
                        try
                        {
                            var newFolder = Global.FileStorageService.CreateNewFolder(parentId.ToString(), split[i]);
                            foldersDict.Add(path, newFolder);
                            matchingFileId.Add(newFolder.ID, folder.FileId);
                        }
                        catch (Exception ex)
                        {
                            Log($"Couldn't create folder {path}", ex);
                        }
                    }
                }
            }

            if (files != null)
            {
                foreach (var file in files)
                {
                    string[] maskPaths = file.Path.Split('/');
                    if (maskPaths[0] == "OwnCloud’s Files " + DateTime.Now.ToString("dd.MM.yyyy"))
                    {
                        maskPaths[0] = "files";
                    }
                    var maskPath = string.Join(Path.DirectorySeparatorChar.ToString(), maskPaths);
                    var parentPath = Path.GetDirectoryName(file.Path);
                    try
                    {
                        var realPath = Path.Combine(drivePath, maskPath);
                        using (var fs = new FileStream(realPath, FileMode.Open))
                        using (var fileDao = Global.DaoFactory.GetFileDao())
                        using (var folderDao = Global.DaoFactory.GetFolderDao())
                        {
                            var parentFolder = string.IsNullOrWhiteSpace(parentPath) ? folderDao.GetFolder(Global.FolderMy) : foldersDict[parentPath];

                            var newFile = new ASCFile
                            {
                                FolderID = parentFolder.ID,
                                Comment = FilesCommonResource.CommentCreate,
                                Title = Path.GetFileName(file.Path),
                                ContentLength = fs.Length
                            };
                            newFile = fileDao.SaveFile(newFile, fs);
                            matchingFileId.Add(newFile.ID, file.FileId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Couldn't create file {parentPath}/{Path.GetFileName(file.Path)}", ex);
                    }
                }
            }

            foreach (var item in matchingFileId)
            {
                var list = new ItemList<AceWrapper>();
                var entryIsFile = files.Exists(el => el.FileId == item.Value) ? true : false;
                var entry = entryIsFile ? files.Find(el => el.FileId == item.Value) : folders.Find(el => el.FileId == item.Value);
                if (entry.Share.Count == 0) continue;
                foreach (var shareInfo in entry.Share)
                {
                    if (shareInfo.ShareWith == null) continue;
                    var shareType = GetPortalShare(shareInfo.Premissions, entryIsFile);
                    users.TryGetValue(shareInfo.ShareWith, out var userToShare);
                    groups.TryGetValue(shareInfo.ShareWith, out var groupToShare);

                    if (userToShare != null || groupToShare != null)
                    {
                        var entryGuid = userToShare == null ? groupToShare.Guid : userToShare.Guid;
                        list.Add(new AceWrapper
                        {
                            Share = shareType.Value,
                            SubjectId = entryGuid,
                            SubjectGroup = false
                        });
                    }
                }
                if (!list.Any()) continue;
                var aceCollection = new AceCollection
                {
                    Entries = new ItemList<string> { (entryIsFile ? "file_" : "folder_") + (int)item.Key },
                    Aces = list,
                    Message = null
                };

                try
                {
                    Global.FileStorageService.SetAceObject(aceCollection, false);
                }
                catch (Exception ex)
                {
                    Log($"Couldn't change file permissions for {aceCollection.Entries.First()}", ex);
                }
            }
        }

        public void SetUsersDict(IEnumerable<OCMigratingUser> users)
        {
            this.users = users.ToDictionary(user => user.Key, user => user);
        }

        public void SetGroupsDict(IEnumerable<OCMigratingGroups> groups)
        {
            this.groups = groups.ToDictionary(group => group.GroupName, group => group);
        }
        private ASCShare? GetPortalShare(int role, bool entryType)
        {
            if (entryType)
            {
                if (role == 1 || role == 17)
                    return ASCShare.Read;
                return ASCShare.ReadWrite;//permission = 19 => denySharing = true, permission = 3 => denySharing = false; ASCShare.ReadWrite
            }
            else
            {
                if (Array.Exists(new int[] { 1, 17, 9, 25, 5, 21, 13, 29, 3, 19, 11, 27 }, el => el == role))
                    return ASCShare.Read;
                return ASCShare.ReadWrite;//permission = 19||23 => denySharing = true, permission = 7||15 => denySharing = false; ASCShare.ReadWrite
            }
        }
    }
}
