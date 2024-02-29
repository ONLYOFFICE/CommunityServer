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
using System.Threading;

using ASC.Common;
using ASC.Common.Security.Authentication;
using ASC.Common.Web;
using ASC.Core;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core.Compress;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;

using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    class FileDownloadOperation : FileOperation
    {
        private readonly Dictionary<object, string> files;
        private readonly Dictionary<string, string> headers;

        public override FileOperationType OperationType
        {
            get { return FileOperationType.Download; }
        }


        public FileDownloadOperation(Dictionary<object, string> folders, Dictionary<object, string> files, Dictionary<string, string> headers, IEnumerable<System.Web.HttpCookie> httpCookies)
            : base(folders.Select(f => f.Key).ToList(), files.Select(f => f.Key).ToList(), cookies: httpCookies)
        {
            this.files = files;
            this.headers = headers;
        }


        protected override void Do()
        {
            List<File> filesForSend;
            List<Folder> folderForSend;

            var entriesPathId = GetEntriesPathId(out filesForSend, out folderForSend);

            if (entriesPathId == null || entriesPathId.Count == 0)
            {
                if (0 < Files.Count)
                    throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
                throw new DirectoryNotFoundException(FilesCommonResource.ErrorMassage_FolderNotFound);
            }

            Total = entriesPathId.Count + 1;

            ReplaceLongPath(entriesPathId);

            using (var stream = CompressTo(entriesPathId))
            {
                if (stream != null)
                {
                    stream.Position = 0;

                    string fileName;

                    if (Folders.Count == 1 && Files.Count == 0)
                    {
                        fileName = String.Format(@"{0}{1}", FolderDao.GetFolder(Folders[0]).Title, CompressToArchive.Instance.ArchiveExtension);
                    }
                    else
                    {
                        fileName = String.Format(@"{0}-{1}-{2}{3}", CoreContext.TenantManager.GetCurrentTenant().TenantAlias.ToLower(), FileConstant.DownloadTitle, DateTime.UtcNow.ToString("yyyy-MM-dd"), CompressToArchive.Instance.ArchiveExtension);
                    }

                    var store = Global.GetStore();

                    string path = null;
                    string sessionKey = null;

                    if (SecurityContext.IsAuthenticated)
                    {
                        path = string.Format(@"{0}\{1}", SecurityContext.CurrentAccount.ID, fileName);
                    }
                    else if (FileShareLink.TryGetSessionId(out var id) && FileShareLink.TryGetCurrentLinkId(out var linkId))
                    {
                        path = string.Format(@"{0}\{1}\{2}", linkId, id, fileName);
                        sessionKey = FileShareLink.CreateDownloadSessionKey(linkId, id);
                    }
                    else
                    {
                        throw new System.Security.SecurityException(FilesCommonResource.ErrorMassage_SecurityException);
                    }

                    if (store.IsFile(FileConstant.StorageDomainTmp, path))
                    {
                        store.Delete(FileConstant.StorageDomainTmp, path);
                    }

                    store.Save(
                        FileConstant.StorageDomainTmp,
                        path,
                        stream,
                        MimeMapping.GetMimeMapping(path),
                        "attachment; filename=\"" + Uri.EscapeDataString(fileName) + "\"");

                    Status = string.Format("{0}?{1}=bulk&filename={2}", FilesLinkUtility.FileHandlerPath, FilesLinkUtility.Action, Uri.EscapeDataString(InstanceCrypto.Encrypt(fileName)));

                    if (!SecurityContext.IsAuthenticated)
                    {
                        Status += $"&session={System.Web.HttpUtility.UrlEncode(sessionKey)}";
                    }

                    ProgressStep();
                }
            }

            foreach (var file in filesForSend)
            {
                var key = file.ID.ToString();
                if (files.ContainsKey(key) && !string.IsNullOrEmpty(files[key]))
                {
                    FilesMessageService.Send(file, headers, MessageAction.FileDownloadedAs, file.Title, files[key]);
                }
                else
                {
                    FilesMessageService.Send(file, headers, MessageAction.FileDownloaded, file.Title);
                }
            }

            foreach (var folder in folderForSend)
            {
                FilesMessageService.Send(folder, headers, MessageAction.FolderDownloaded, folder.Title);
            }
        }

        private ItemNameValueCollection ExecPathFromFile(File file, string path)
        {
            FileMarker.RemoveMarkAsNew(file);

            var title = file.Title;

            if (files.ContainsKey(file.ID.ToString()))
            {
                var convertToExt = files[file.ID.ToString()];

                if (!string.IsNullOrEmpty(convertToExt))
                {
                    title = FileUtility.ReplaceFileExtension(title, convertToExt);
                }
            }

            var entriesPathId = new ItemNameValueCollection();
            entriesPathId.Add(path + title, file.ID.ToString());

            return entriesPathId;
        }

        private ItemNameValueCollection GetEntriesPathId(out List<File> filesForSend, out List<Folder> folderForSend)
        {
            filesForSend = new List<File>();
            folderForSend = new List<Folder>();

            var entriesPathId = new ItemNameValueCollection();

            if (0 < Files.Count)
            {
                filesForSend = FilesSecurity.FilterDownload(FileDao.GetFiles(Files));
                filesForSend.ForEach(file => entriesPathId.Add(ExecPathFromFile(file, string.Empty)));
            }
            if (0 < Folders.Count)
            {
                folderForSend = FolderDao.GetFolders(Folders);
                folderForSend = FilesSecurity.FilterDownload(folderForSend);
                folderForSend.ForEach(folder => FileMarker.RemoveMarkAsNew(folder));

                var filesInFolder = GetFilesInFolders(folderForSend.Select(x => x.ID), string.Empty);
                entriesPathId.Add(filesInFolder);
            }

            if (Folders.Count == 1 && Files.Count == 0)
            {
                var entriesPathIdWithoutRoot = new ItemNameValueCollection();

                foreach (var path in entriesPathId.AllKeys)
                {
                    entriesPathIdWithoutRoot.Add(path.Remove(0, path.IndexOf('/') + 1), entriesPathId[path]);
                }

                return entriesPathIdWithoutRoot;
            }

            return entriesPathId;
        }

        private ItemNameValueCollection GetFilesInFolders(IEnumerable<object> folderIds, string path)
        {
            CancellationToken.ThrowIfCancellationRequested();

            var entriesPathId = new ItemNameValueCollection();
            foreach (var folderId in folderIds)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var folder = FolderDao.GetFolder(folderId);
                if (folder == null || !FilesSecurity.CanDownload(folder)) continue;
                var folderPath = path + folder.Title + "/";

                var files = FileDao.GetFiles(folder.ID, null, FilterType.None, false, Guid.Empty, string.Empty, true, null);
                files = FilesSecurity.FilterDownload(files);
                files.ForEach(file => entriesPathId.Add(ExecPathFromFile(file, folderPath)));

                FileMarker.RemoveMarkAsNew(folder);

                var nestedFolders = FolderDao.GetFolders(folder.ID);
                nestedFolders = FilesSecurity.FilterDownload(nestedFolders);
                if (files.Count == 0 && nestedFolders.Count == 0)
                {
                    entriesPathId.Add(folderPath, String.Empty);
                }

                var filesInFolder = GetFilesInFolders(nestedFolders.ConvertAll(f => f.ID), folderPath);
                entriesPathId.Add(filesInFolder);
            }
            return entriesPathId;
        }

        private Stream CompressTo(ItemNameValueCollection entriesPathId)
        {
            var stream = TempStream.Create();

            using (ICompress compressTo = new CompressToArchive(stream))
            {
                foreach (var path in entriesPathId.AllKeys)
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        ProgressStep();
                        continue;
                    }

                    var counter = 0;
                    foreach (var entryId in entriesPathId[path])
                    {
                        if (CancellationToken.IsCancellationRequested)
                        {
                            compressTo.Dispose();
                            stream.Dispose();
                            CancellationToken.ThrowIfCancellationRequested();
                        }

                        var newtitle = path;

                        File file = null;
                        var convertToExt = string.Empty;

                        if (!string.IsNullOrEmpty(entryId))
                        {
                            FileDao.InvalidateCache(entryId);
                            file = FileDao.GetFile(entryId);

                            if (file == null)
                            {
                                Error = FilesCommonResource.ErrorMassage_FileNotFound;
                                continue;
                            }

                            if (files.ContainsKey(file.ID.ToString()))
                            {
                                convertToExt = files[file.ID.ToString()];
                                if (!string.IsNullOrEmpty(convertToExt))
                                {
                                    newtitle = FileUtility.ReplaceFileExtension(path, convertToExt);
                                }
                            }
                        }

                        if (0 < counter)
                        {
                            var suffix = " (" + counter + ")";

                            if (!string.IsNullOrEmpty(entryId))
                            {
                                newtitle = 0 < newtitle.IndexOf('.') ? newtitle.Insert(newtitle.LastIndexOf('.'), suffix) : newtitle + suffix;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(entryId) && file != null)
                        {
                            compressTo.CreateEntry(newtitle, file.ModifiedOn);

                            try
                            {
                                if (FileConverter.EnableConvert(file, convertToExt))
                                {
                                    //Take from converter
                                    using (var readStream = FileConverter.Exec(file, convertToExt))
                                    {
                                        compressTo.PutStream(readStream);
                                    }
                                }
                                else
                                {
                                    using (var readStream = FileDao.GetFileStream(file))
                                    {
                                        compressTo.PutStream(readStream);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Error = ex.Message;
                                Logger.Error(Error, ex);
                            }
                        }
                        else
                        {
                            compressTo.CreateEntry(newtitle);

                            compressTo.PutNextEntry();
                        }

                        compressTo.CloseEntry();
                        counter++;

                        if (!string.IsNullOrEmpty(entryId) && file != null)
                        {
                            ProcessedFile(entryId);
                        }
                        else
                        {
                            ProcessedFolder(default(object));
                        }

                    }
                    ProgressStep();
                }
            }
            return stream;
        }

        private void ReplaceLongPath(ItemNameValueCollection entriesPathId)
        {
            foreach (var path in new List<string>(entriesPathId.AllKeys))
            {
                CancellationToken.ThrowIfCancellationRequested();

                if (200 >= path.Length || 0 >= path.IndexOf('/')) continue;

                var ids = entriesPathId[path];
                entriesPathId.Remove(path);

                var newtitle = "LONG_FOLDER_NAME" + path.Substring(path.LastIndexOf('/'));
                entriesPathId.Add(newtitle, ids);
            }
        }


        class ItemNameValueCollection
        {
            private readonly Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();


            public IEnumerable<string> AllKeys
            {
                get { return dic.Keys; }
            }

            public IEnumerable<string> this[string name]
            {
                get { return dic[name].ToArray(); }
            }

            public int Count
            {
                get { return dic.Count; }
            }

            public void Add(string name, string value)
            {
                if (!dic.ContainsKey(name))
                {
                    dic.Add(name, new List<string>());
                }
                dic[name].Add(value);
            }

            public void Add(ItemNameValueCollection collection)
            {
                foreach (var key in collection.AllKeys)
                {
                    foreach (var value in collection[key])
                    {
                        Add(key, value);
                    }
                }
            }

            public void Add(string name, IEnumerable<string> values)
            {
                if (!dic.ContainsKey(name))
                {
                    dic.Add(name, new List<string>());
                }
                dic[name].AddRange(values);
            }

            public void Remove(string name)
            {
                dic.Remove(name);
            }
        }
    }
}