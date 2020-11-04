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
using System.Text;
using System.Threading;

using ASC.Common.Security.Authentication;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;

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


        public FileDownloadOperation(Dictionary<object, string> folders, Dictionary<object, string> files, Dictionary<string, string> headers)
            : base(folders.Select(f => f.Key).ToList(), files.Select(f => f.Key).ToList())
        {
            this.files = files;
            this.headers = headers;
        }


        protected override void Do()
        {
            var entriesPathId = GetEntriesPathId();
            if (entriesPathId == null || entriesPathId.Count == 0)
            {
                if (0 < Files.Count)
                    throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
                throw new DirectoryNotFoundException(FilesCommonResource.ErrorMassage_FolderNotFound);
            }

            ReplaceLongPath(entriesPathId);

            using (var stream = CompressToZip(entriesPathId))
            {
                if (stream != null)
                {
                    stream.Position = 0;
                    const string fileName = FileConstant.DownloadTitle + ".zip";
                    var store = Global.GetStore();
                    store.Save(
                        FileConstant.StorageDomainTmp,
                        string.Format(@"{0}\{1}", ((IAccount)Thread.CurrentPrincipal.Identity).ID, fileName),
                        stream,
                        "application/zip",
                        "attachment; filename=\"" + fileName + "\"");
                    Status = string.Format("{0}?{1}=bulk", FilesLinkUtility.FileHandlerPath, FilesLinkUtility.Action);
                }
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

        private ItemNameValueCollection GetEntriesPathId()
        {
            var entriesPathId = new ItemNameValueCollection();
            if (0 < Files.Count)
            {
                var files = FileDao.GetFiles(Files.ToArray());
                files = FilesSecurity.FilterRead(files).ToList();
                files.ForEach(file => entriesPathId.Add(ExecPathFromFile(file, string.Empty)));
            }
            if (0 < Folders.Count)
            {
                FilesSecurity.FilterRead(FolderDao.GetFolders(Files.ToArray())).ToList().Cast<FileEntry>().ToList()
                             .ForEach(folder => FileMarker.RemoveMarkAsNew(folder));

                var filesInFolder = GetFilesInFolders(Folders, string.Empty);
                entriesPathId.Add(filesInFolder);
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
                if (folder == null || !FilesSecurity.CanRead(folder)) continue;
                var folderPath = path + folder.Title + "/";

                var files = FileDao.GetFiles(folder.ID, null, FilterType.None, false, Guid.Empty, string.Empty, true);
                files = FilesSecurity.FilterRead(files).ToList();
                files.ForEach(file => entriesPathId.Add(ExecPathFromFile(file, folderPath)));

                FileMarker.RemoveMarkAsNew(folder);

                var nestedFolders = FolderDao.GetFolders(folder.ID);
                nestedFolders = FilesSecurity.FilterRead(nestedFolders).ToList();
                if (files.Count == 0 && nestedFolders.Count == 0)
                {
                    entriesPathId.Add(folderPath, String.Empty);
                }

                var filesInFolder = GetFilesInFolders(nestedFolders.ConvertAll(f => f.ID), folderPath);
                entriesPathId.Add(filesInFolder);
            }
            return entriesPathId;
        }

        private Stream CompressToZip(ItemNameValueCollection entriesPathId)
        {
            var stream = TempStream.Create();
            using (var zip = new Ionic.Zip.ZipOutputStream(stream, true))
            {
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level3;
                zip.AlternateEncodingUsage = Ionic.Zip.ZipOption.AsNecessary;
                zip.AlternateEncoding = Encoding.UTF8;

                foreach (var path in entriesPathId.AllKeys)
                {
                    var counter = 0;
                    foreach (var entryId in entriesPathId[path])
                    {
                        if (CancellationToken.IsCancellationRequested)
                        {
                            zip.Dispose();
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

                            if (file.ContentLength > SetupInfo.AvailableFileSize)
                            {
                                Error = string.Format(FilesCommonResource.ErrorMassage_FileSizeZip, FileSizeComment.FilesSizeToString(SetupInfo.AvailableFileSize));
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

                        zip.PutNextEntry(newtitle);

                        if (!string.IsNullOrEmpty(entryId) && file != null)
                        {
                            try
                            {
                                if (FileConverter.EnableConvert(file, convertToExt))
                                {
                                    //Take from converter
                                    using (var readStream = FileConverter.Exec(file, convertToExt))
                                    {
                                        readStream.StreamCopyTo(zip);
                                        if (!string.IsNullOrEmpty(convertToExt))
                                        {
                                            FilesMessageService.Send(file, headers, MessageAction.FileDownloadedAs, file.Title, convertToExt);
                                        }
                                        else
                                        {
                                            FilesMessageService.Send(file, headers, MessageAction.FileDownloaded, file.Title);
                                        }
                                    }
                                }
                                else
                                {
                                    using (var readStream = FileDao.GetFileStream(file))
                                    {
                                        readStream.StreamCopyTo(zip);
                                        FilesMessageService.Send(file, headers, MessageAction.FileDownloaded, file.Title);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Error = ex.Message;
                                Logger.Error(Error, ex);
                            }
                        }
                        counter++;
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