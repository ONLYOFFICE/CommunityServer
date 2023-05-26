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


using System.Collections.Generic;
using System.Linq;
using System.Threading;

using ASC.Common.Security.Authentication;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Utils;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    class FileMarkAsReadOperation : FileOperation
    {
        private readonly Dictionary<string, string> headers;

        public override FileOperationType OperationType
        {
            get { return FileOperationType.MarkAsRead; }
        }


        public FileMarkAsReadOperation(List<object> folders, List<object> files, Dictionary<string, string> headers)
            : base(folders, files)
        {
            this.headers = headers;
        }


        protected override int InitTotalProgressSteps()
        {
            return Files.Count + Folders.Count;
        }

        protected override void Do()
        {
            var entries = new List<FileEntry>();
            if (Folders.Any())
            {
                entries.AddRange(FolderDao.GetFolders(Folders));
            }
            if (Files.Any())
            {
                entries.AddRange(FileDao.GetFiles(Files));
            }
            entries.ForEach(entry =>
            {
                CancellationToken.ThrowIfCancellationRequested();

                FileMarker.RemoveMarkAsNew(entry, ((IAccount)Thread.CurrentPrincipal.Identity).ID);

                if (entry.FileEntryType == FileEntryType.File)
                {
                    ProcessedFile(entry.ID.ToString());
                    FilesMessageService.Send(entry, headers, MessageAction.FileMarkedAsRead, entry.Title);
                }
                else
                {
                    ProcessedFolder(entry.ID.ToString());
                    FilesMessageService.Send(entry, headers, MessageAction.FolderMarkedAsRead, entry.Title);
                }
                ProgressStep();
            });

            var newrootfolder = FileMarker
                .GetRootFoldersIdMarkedAsNew()
                .Select(item => string.Format("new_{{\"key\"? \"{0}\", \"value\"? \"{1}\"}}", item.Key, item.Value));

            Status += string.Join(SPLIT_CHAR, newrootfolder.ToArray());
        }
    }
}