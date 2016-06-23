/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


extern alias ionic;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
                var convertToExt = string.Empty;
                if (FileUtility.InternalExtension.Values.Contains(convertToExt))
                    convertToExt = files[file.ID.ToString()];

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
                var folder = FolderDao.GetFolder(folderId);
                if (folder == null || !FilesSecurity.CanRead(folder)) continue;
                var folderPath = path + folder.Title + "/";

                var files = FileDao.GetFiles(folder.ID, null, FilterType.None, Guid.Empty, string.Empty);
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
            using (var zip = new ionic::Ionic.Zip.ZipOutputStream(stream, true))
            {
                zip.CompressionLevel = ionic::Ionic.Zlib.CompressionLevel.Level3;
                zip.AlternateEncodingUsage = ionic::Ionic.Zip.ZipOption.AsNecessary;
                zip.AlternateEncoding = Encoding.GetEncoding(Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage);

                foreach (var path in entriesPathId.AllKeys)
                {
                    if (CancellationToken.IsCancellationRequested)
                    {
                        zip.Dispose();
                        stream.Dispose();
                        CancellationToken.ThrowIfCancellationRequested();
                    }

                    var counter = 0;
                    foreach (var entryId in entriesPathId[path])
                    {
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
                            if (FileConverter.EnableConvert(file, convertToExt))
                            {
                                //Take from converter
                                try
                                {
                                    using (var readStream = !string.IsNullOrEmpty(convertToExt) ? FileConverter.Exec(file, convertToExt) : FileConverter.Exec(file))
                                    {
                                        if (readStream != null)
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
                                }
                                catch (Exception ex)
                                {
                                    Error = ex.Message;
                                    Logger.Error(Error, ex);
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
                        counter++;
                    }

                    ProgressStep();
                }
            }
            return stream;
        }

        private static void ReplaceLongPath(ItemNameValueCollection entriesPathId)
        {
            foreach (var path in new List<string>(entriesPathId.AllKeys))
            {
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