/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using ASC.Common.Caching;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Web.Files.Classes;
using Microsoft.SharePoint.Client;
using File = Microsoft.SharePoint.Client.File;
using Folder = Microsoft.SharePoint.Client.Folder;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Files.Thirdparty.SharePoint
{
    public class SharePointProviderInfo : IProviderInfo, IDisposable
    {
        private ClientContext clientContext;

        public int ID { get; set; }
        public string ProviderKey { get; private set; }
        public Guid Owner { get; private set; }
        public FolderType RootFolderType { get; private set; }
        public DateTime CreateOn { get; private set; }
        public string CustomerTitle { get; private set; }
        public object RootFolderId { get; private set; }

        public string SpRootFolderId = "/Shared Documents";


        public SharePointProviderInfo(int id, string providerKey, string customerTitle, AuthData authData, Guid owner,
                                      FolderType rootFolderType, DateTime createOn)
        {
            if (string.IsNullOrEmpty(providerKey))
                throw new ArgumentNullException("providerKey");
            if (!string.IsNullOrEmpty(authData.Login) && string.IsNullOrEmpty(authData.Password))
                throw new ArgumentNullException("password", "Password can't be null");

            ID = id;
            ProviderKey = providerKey;
            CustomerTitle = customerTitle;
            Owner = owner == Guid.Empty ? SecurityContext.CurrentAccount.ID : owner;
            RootFolderType = rootFolderType;
            CreateOn = createOn;
            RootFolderId = MakeId();

            InitClientContext(authData);
        }


        public bool CheckAccess()
        {
            try
            {
                clientContext.Load(clientContext.Web);
                clientContext.ExecuteQuery();
                return true;
            }
            catch (Exception e)
            {
                Global.Logger.Warn("CheckAccess", e);
                return false;
            }
        }

        public void InvalidateStorage()
        {
            clientContext.Dispose();
            Notify.Publish(new SharePointProviderCacheItem(), CacheNotifyAction.Remove);
        }

        internal void UpdateTitle(string newtitle)
        {
            CustomerTitle = newtitle;
        }

        private void InitClientContext(AuthData authData)
        {
            var authUrl = authData.Url;
            ICredentials credentials = new NetworkCredential(authData.Login, authData.Password);

            if (authData.Login.EndsWith("onmicrosoft.com"))
            {
                var personalPath = string.Concat("/personal/", authData.Login.Replace("@", "_").Replace(".", "_").ToLower());
                SpRootFolderId = string.Concat(personalPath, "/Documents");

                var ss = new SecureString();
                foreach (var p in authData.Password)
                {
                    ss.AppendChar(p);
                }
                authUrl = string.Concat(authData.Url.TrimEnd('/'), personalPath);
                credentials = new SharePointOnlineCredentials(authData.Login, ss);

            }

            clientContext = new ClientContext(authUrl)
                {
                    AuthenticationMode = ClientAuthenticationMode.Default,
                    Credentials = credentials
                };
        }

        #region Files

        public File GetFileById(object id)
        {
            var key = "spointf-" + MakeId(id);
            var file = FileCache.Get<File>(key);
            if (file == null)
            {
                file = GetFile(id);
                if (file != null)
                    FileCache.Insert(key, file, DateTime.UtcNow.Add(CacheExpiration));
            }
            return file;
        }

        private File GetFile(object id)
        {
            var file = clientContext.Web.GetFileByServerRelativeUrl((string)id);
            clientContext.Load(file);
            clientContext.Load(file.ListItemAllFields);

            try
            {
                clientContext.ExecuteQuery();
            }
            catch (Exception ex)
            {
                Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(GetParentFolderId(id)) }, CacheNotifyAction.Remove);
                var serverException = (ServerException)ex;
                if (serverException.ServerErrorTypeName == (typeof(FileNotFoundException)).ToString())
                {
                    return null;
                }
                return new SharePointFileErrorEntry(file.Context, file.Path) { Error = ex.Message, ID = id };
            }

            return file;
        }

        public Stream GetFileStream(object id, int offset = 0)
        {
            var file = GetFileById(id);

            if (file is SharePointFileErrorEntry) return null;
            var fileInfo = File.OpenBinaryDirect(clientContext, (string)id);
            clientContext.ExecuteQuery();

            var tempBuffer = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 8096, FileOptions.DeleteOnClose);
            using (var str = fileInfo.Stream)
            {
                if (str != null)
                {
                    str.CopyTo(tempBuffer);
                    tempBuffer.Flush();
                    tempBuffer.Seek(offset, SeekOrigin.Begin);
                }
            }

            return tempBuffer;
        }

        public File CreateFile(string id, Stream stream)
        {
            byte[] b;

            using (var br = new BinaryReader(stream))
            {
                b = br.ReadBytes((int)stream.Length);
            }

            var file = clientContext.Web.RootFolder.Files.Add(new FileCreationInformation { Content = b, Url = id, Overwrite = true });
            clientContext.Load(file);
            clientContext.Load(file.ListItemAllFields);
            clientContext.ExecuteQuery();

            FileCache.Insert("spointf-" + MakeId(id), file, DateTime.UtcNow.Add(CacheExpiration));
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(GetParentFolderId(id)) }, CacheNotifyAction.Remove);

            return file;
        }

        public void DeleteFile(string id)
        {
            Notify.Publish(new SharePointProviderCacheItem { FileKey = MakeId(id), FolderKey = MakeId(GetParentFolderId(id)) }, CacheNotifyAction.Remove);

            var file = GetFileById(id);

            if (file is SharePointFileErrorEntry) return;

            file.DeleteObject();
            clientContext.ExecuteQuery();
        }

        public object RenameFile(string id, string newTitle)
        {
            Notify.Publish(new SharePointProviderCacheItem { FileKey = MakeId(id), FolderKey = MakeId(GetParentFolderId(id)) }, CacheNotifyAction.Remove);

            var file = GetFileById(id);

            if (file is SharePointFileErrorEntry) return MakeId();

            var newUrl = GetParentFolderId(file.ServerRelativeUrl) + "/" + newTitle;
            file.MoveTo(newUrl, MoveOperations.Overwrite);
            clientContext.ExecuteQuery();

            return MakeId(newUrl);
        }

        public object MoveFile(object id, object toFolderId)
        {
            Notify.Publish(new SharePointProviderCacheItem { FileKey = MakeId(id), FolderKey = MakeId(GetParentFolderId(id)) }, CacheNotifyAction.Remove);
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(toFolderId) }, CacheNotifyAction.Remove);

            var file = GetFileById(id);

            if (file is SharePointFileErrorEntry) return MakeId();

            var newUrl = toFolderId + "/" + file.Name;
            file.MoveTo(newUrl, MoveOperations.Overwrite);
            clientContext.ExecuteQuery();

            return MakeId(newUrl);
        }

        public File CopyFile(object id, object toFolderId)
        {
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(GetParentFolderId(id)) }, CacheNotifyAction.Remove);
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(toFolderId) }, CacheNotifyAction.Remove);

            var file = GetFileById(id);

            if (file is SharePointFileErrorEntry) return file;

            var newUrl = toFolderId + "/" + file.Name;
            file.CopyTo(newUrl, false);
            clientContext.ExecuteQuery();

            return file;
        }

        public Core.File ToFile(File file)
        {
            if (file == null)
                return null;

            var errorFile = file as SharePointFileErrorEntry;
            if (errorFile != null)
                return new Core.File
                    {
                        ID = MakeId(errorFile.ID),
                        FolderID = MakeId(GetParentFolderId(errorFile.ID)),
                        CreateBy = Owner,
                        CreateOn = DateTime.UtcNow,
                        ModifiedBy = Owner,
                        ModifiedOn = DateTime.UtcNow,
                        ProviderId = ID,
                        ProviderKey = ProviderKey,
                        RootFolderCreator = Owner,
                        RootFolderId = MakeId(RootFolder.ServerRelativeUrl),
                        RootFolderType = RootFolderType,
                        Title = MakeTitle(GetTitleById(errorFile.ID)),
                        Error = errorFile.Error
                    };

            var result = new Core.File
                {
                    ID = MakeId(file.ServerRelativeUrl),
                    Access = Core.Security.FileShare.None,
                    //ContentLength = file.Length,
                    CreateBy = Owner,
                    CreateOn = file.TimeCreated.Kind == DateTimeKind.Utc ? TenantUtil.DateTimeFromUtc(file.TimeCreated) : file.TimeCreated,
                    FileStatus = FileStatus.None,
                    FolderID = MakeId(GetParentFolderId(file.ServerRelativeUrl)),
                    ModifiedBy = Owner,
                    ModifiedOn = file.TimeLastModified.Kind == DateTimeKind.Utc ? TenantUtil.DateTimeFromUtc(file.TimeLastModified) : file.TimeLastModified,
                    NativeAccessor = file,
                    ProviderId = ID,
                    ProviderKey = ProviderKey,
                    Title = MakeTitle(file.Name),
                    RootFolderId = MakeId(SpRootFolderId),
                    RootFolderType = RootFolderType,
                    RootFolderCreator = Owner,
                    Shared = false,
                    Version = 1
                };

            if (file.IsPropertyAvailable("Length"))
            {
                result.ContentLength = file.Length;
            }
            else if (file.IsObjectPropertyInstantiated("ListItemAllFields"))
            {
                result.ContentLength = Convert.ToInt64(file.ListItemAllFields["File_x0020_Size"]);
            }

            return result;
        }

        #endregion

        #region Folders

        public Folder RootFolder
        {
            get
            {
                var key = "spointd-" + MakeId();
                var folder = FolderCache.Get<Folder>(key);
                if (folder == null)
                {
                    folder = GetFolderById(SpRootFolderId);
                    FolderCache.Insert(key, folder, DateTime.UtcNow.Add(CacheExpiration));
                }
                return folder;
            }
        }

        public Folder GetFolderById(object id)
        {
            var key = "spointd-" + MakeId(id);
            var folder = FolderCache.Get<Folder>(key);
            if (folder == null)
            {
                folder = GetFolder(id);
                if (folder != null)
                    FolderCache.Insert(key, folder, DateTime.UtcNow.Add(CacheExpiration));
            }
            return folder;
        }

        private Folder GetFolder(object id)
        {
            if ((string)id == "") id = SpRootFolderId;
            var folder = clientContext.Web.GetFolderByServerRelativeUrl((string)id);
            clientContext.Load(folder);
            clientContext.Load(folder.Files, collection => collection.IncludeWithDefaultProperties(r => r.ListItemAllFields));
            clientContext.Load(folder.Folders);

            try
            {
                clientContext.ExecuteQuery();
            }
            catch (Exception ex)
            {
                Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(GetParentFolderId(id)) }, CacheNotifyAction.Remove);
                var serverException = (ServerException)ex;
                if (serverException.ServerErrorTypeName == (typeof (FileNotFoundException)).ToString())
                {
                    return null;
                }
                return new SharePointFolderErrorEntry(folder.Context, folder.Path) { Error = ex.Message, ID = id };
            }

            return folder;
        }

        public Folder GetParentFolder(string serverRelativeUrl)
        {
            return GetFolderById(GetParentFolderId(serverRelativeUrl));
        }

        public IEnumerable<File> GetFolderFiles(object id)
        {
            var folder = GetFolderById(id);
            if (folder is SharePointFolderErrorEntry) return new List<File>();

            return GetFolderById(id).Files;
        }

        public IEnumerable<Folder> GetFolderFolders(object id)
        {
            var folder = GetFolderById(id);
            if (folder is SharePointFolderErrorEntry) return new List<Folder>();

            return folder.Folders.ToList().Where(r => r.ServerRelativeUrl != SpRootFolderId + "/" + "Forms");
        }

        public object RenameFolder(object id, string newTitle)
        {
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(GetParentFolderId(id)) }, CacheNotifyAction.Remove);
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(id) }, CacheNotifyAction.Remove);

            var folder = GetFolderById(id);
            if (folder is SharePointFolderErrorEntry) return MakeId(id);

            return MakeId(MoveFld(folder, GetParentFolderId(id) + "/" + newTitle).ServerRelativeUrl);
        }

        public object MoveFolder(object id, object toFolderId)
        {
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(id) }, CacheNotifyAction.Remove);
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(GetParentFolderId(id)) }, CacheNotifyAction.Remove);
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(toFolderId) }, CacheNotifyAction.Remove);

            var folder = GetFolderById(id);
            if (folder is SharePointFolderErrorEntry) return MakeId(id);

            return MakeId(MoveFld(folder, toFolderId + "/" + GetFolderById(id).Name).ServerRelativeUrl);
        }

        public Folder CopyFolder(object id, object toFolderId)
        {
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(toFolderId) }, CacheNotifyAction.Remove);

            var folder = GetFolderById(id);
            if (folder is SharePointFolderErrorEntry) return folder;

            return MoveFld(folder, toFolderId + "/" + GetFolderById(id).Name, false);
        }

        private Folder MoveFld(Folder folder, string newUrl, bool delete = true)
        {
            var newFolder = CreateFolder(newUrl);

            if (delete)
            {
                folder.Folders.ToList().ForEach(r => MoveFolder(r.ServerRelativeUrl, newUrl));
                folder.Files.ToList().ForEach(r => MoveFile(r.ServerRelativeUrl, newUrl));

                folder.DeleteObject();
                clientContext.ExecuteQuery();
            }
            else
            {
                folder.Folders.ToList().ForEach(r => CopyFolder(r.ServerRelativeUrl, newUrl));
                folder.Files.ToList().ForEach(r => CopyFile(r.ServerRelativeUrl, newUrl));
            }

            return newFolder;
        }

        public Folder CreateFolder(string id)
        {
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(GetParentFolderId(id)) }, CacheNotifyAction.Remove);

            var folder = clientContext.Web.RootFolder.Folders.Add(id);
            clientContext.Load(folder);
            clientContext.ExecuteQuery();

            FolderCache.Insert("spointd-" + id, folder, DateTime.UtcNow.Add(CacheExpiration));

            return folder;
        }

        public void DeleteFolder(string id)
        {
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(id) }, CacheNotifyAction.Remove);
            Notify.Publish(new SharePointProviderCacheItem { FolderKey = MakeId(GetParentFolderId(id)) }, CacheNotifyAction.Remove);

            var folder = GetFolderById(id);

            if (folder is SharePointFolderErrorEntry) return;

            folder.DeleteObject();
            clientContext.ExecuteQuery();
        }

        public Core.Folder ToFolder(Folder folder)
        {
            if (folder == null) return null;

            var errorFolder = folder as SharePointFolderErrorEntry;
            if (errorFolder != null)
                return new Core.Folder
                    {
                        ID = MakeId(errorFolder.ID),
                        ParentFolderID = null,
                        CreateBy = Owner,
                        CreateOn = DateTime.UtcNow,
                        FolderType = FolderType.DEFAULT,
                        ModifiedBy = Owner,
                        ModifiedOn = DateTime.UtcNow,
                        ProviderId = ID,
                        ProviderKey = ProviderKey,
                        RootFolderCreator = Owner,
                        RootFolderId = MakeId(SpRootFolderId),
                        RootFolderType = RootFolderType,
                        Shareable = false,
                        Title = MakeTitle(GetTitleById(errorFolder.ID)),
                        TotalFiles = 0,
                        TotalSubFolders = 0,
                        Error = errorFolder.Error
                    };

            var isRoot = folder.ServerRelativeUrl == SpRootFolderId;
            return new Core.Folder
                {
                    ID = MakeId(isRoot ? "" : folder.ServerRelativeUrl),
                    ParentFolderID = isRoot ? null : MakeId(GetParentFolderId(folder.ServerRelativeUrl)),
                    CreateBy = Owner,
                    CreateOn = CreateOn,
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = Owner,
                    ModifiedOn = CreateOn,
                    ProviderId = ID,
                    ProviderKey = ProviderKey,
                    RootFolderCreator = Owner,
                    RootFolderId = MakeId(RootFolder.ServerRelativeUrl),
                    RootFolderType = RootFolderType,
                    Shareable = false,
                    Title = isRoot ? CustomerTitle : MakeTitle(folder.Name),
                    TotalFiles = 0,
                    TotalSubFolders = 0,
                };
        }

        #endregion

        public string MakeId(string path = "")
        {
            path = path.Replace(SpRootFolderId, "");
            return string.Format("{0}{1}", "spoint-" + ID, string.IsNullOrEmpty(path) || path == "/" || path == SpRootFolderId ? "" : ("-" + path.Replace('/', '|')));
        }

        private string MakeId(object path)
        {
            return MakeId((string)path);
        }

        protected string MakeTitle(string name)
        {
            return Global.ReplaceInvalidCharsAndTruncate(name);
        }

        protected string GetParentFolderId(string serverRelativeUrl)
        {
            var path = serverRelativeUrl.Split('/');

            return string.Join("/", path.Take(path.Length - 1));
        }

        protected string GetParentFolderId(object serverRelativeUrl)
        {
            return GetParentFolderId((string)serverRelativeUrl);
        }

        protected string GetTitleById(object serverRelativeUrl)
        {
            return ((string)serverRelativeUrl).Split('/').Last();
        }


        public void Dispose()
        {
            clientContext.Dispose();
        }


        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(1);
        private static readonly ICache FileCache = AscCache.Memory;
        private static readonly ICache FolderCache = AscCache.Memory;
        private static readonly ICacheNotify Notify = AscCache.Notify;

        static SharePointProviderInfo()
        {
            Notify.Subscribe<SharePointProviderCacheItem>((i, action) =>
                {
                    if (action != CacheNotifyAction.Remove) return;
                    if (!string.IsNullOrEmpty(i.FileKey))
                    {
                        FileCache.Remove("spointf-" + i.FileKey);
                    }
                    if (!string.IsNullOrEmpty(i.FolderKey))
                    {
                        FolderCache.Remove("spointd-" + i.FolderKey);
                    }
                    if (string.IsNullOrEmpty(i.FileKey) && string.IsNullOrEmpty(i.FolderKey))
                    {
                        FileCache.Remove(new Regex("^spointf-.*"));
                        FolderCache.Remove(new Regex("^spointd-.*"));
                    }
                });
        }

        private class SharePointProviderCacheItem
        {
            public string FileKey { get; set; }

            public string FolderKey { get; set; }
        }
    }
}