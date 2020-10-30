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
using System.Diagnostics;
using System.Linq;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Web.Files.Resources;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    class FileOperationsManager
    {
        private DistributedTaskQueue tasks = new DistributedTaskQueue("fileOperations", 10);
        

        public ItemList<FileOperationResult> GetOperationResults()
        {
            var operations = tasks.GetTasks();
            var processlist = Process.GetProcesses();

            foreach (var o in operations.Where(o => string.IsNullOrEmpty(o.InstanseId)
                                                    || processlist.All(p => p.Id != int.Parse(o.InstanseId))))
            {
                o.SetProperty(FileOperation.PROGRESS, 100);
                tasks.RemoveTask(o.Id);
            }

            operations = operations.Where(t => t.GetProperty<Guid>(FileOperation.OWNER) == SecurityContext.CurrentAccount.ID);
            foreach (var o in operations.Where(o => DistributedTaskStatus.Running < o.Status))
            {
                o.SetProperty(FileOperation.PROGRESS, 100);
                tasks.RemoveTask(o.Id);
            }

            var results = operations
                .Where(o => o.GetProperty<bool>(FileOperation.HOLD) || o.GetProperty<int>(FileOperation.PROGRESS) != 100)
                .Select(o => new FileOperationResult
                    {
                        Id = o.Id,
                        OperationType = o.GetProperty<FileOperationType>(FileOperation.OPERATION_TYPE),
                        Source = o.GetProperty<string>(FileOperation.SOURCE),
                        Progress = o.GetProperty<int>(FileOperation.PROGRESS),
                        Processed = o.GetProperty<int>(FileOperation.PROCESSED).ToString(),
                        Result = o.GetProperty<string>(FileOperation.RESULT),
                        Error = o.GetProperty<string>(FileOperation.ERROR),
                        Finished = o.GetProperty<bool>(FileOperation.FINISHED),
                    });

            return new ItemList<FileOperationResult>(results);
        }

        public ItemList<FileOperationResult> CancelOperations()
        {
            var operations = tasks.GetTasks()
                .Where(t => t.GetProperty<Guid>(FileOperation.OWNER) == SecurityContext.CurrentAccount.ID);

            foreach (var o in operations)
            {
                tasks.CancelTask(o.Id);
            }

            return GetOperationResults();
        }


        public ItemList<FileOperationResult> MarkAsRead(List<object> folderIds, List<object> fileIds)
        {
            var op = new FileMarkAsReadOperation(folderIds, fileIds);
            return QueueTask(op);
        }

        public ItemList<FileOperationResult> Download(Dictionary<object, string> folders, Dictionary<object, string> files, Dictionary<string, string> headers)
        {
            var operations = tasks.GetTasks()
                .Where(t => t.GetProperty<Guid>(FileOperation.OWNER) == SecurityContext.CurrentAccount.ID)
                .Where(t => t.GetProperty<FileOperationType>(FileOperation.OPERATION_TYPE) == FileOperationType.Download);

            if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
            {
                throw new InvalidOperationException(FilesCommonResource.ErrorMassage_ManyDownloads);
            }

            var op = new FileDownloadOperation(folders, files, headers);
            return QueueTask(op);
        }

        public ItemList<FileOperationResult> MoveOrCopy(List<object> folders, List<object> files, string destFolderId, bool copy, FileConflictResolveType resolveType, bool holdResult, Dictionary<string, string> headers)
        {
            var op = new FileMoveCopyOperation(folders, files, destFolderId, copy, resolveType, holdResult, headers);
            return QueueTask(op);
        }

        public ItemList<FileOperationResult> Delete(List<object> folders, List<object> files, bool ignoreException, bool holdResult, bool immediately, Dictionary<string, string> headers)
        {
            var op = new FileDeleteOperation(folders, files, ignoreException, holdResult, immediately, headers);
            return QueueTask(op);
        }


        private ItemList<FileOperationResult> QueueTask(FileOperation op)
        {
            tasks.QueueTask(op.RunJob, op.GetDistributedTask());
            return GetOperationResults();
        }
    }
}