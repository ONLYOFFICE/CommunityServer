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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Studio.Utility;

namespace ASC.Api.Documents
{
    /// <summary>
    /// </summary>
    [DataContract(Name = "operation_result", Namespace = "")]
    public class FileOperationWraper
    {
        /// <summary>
        /// </summary>
        [DataMember(Name = "id", IsRequired = false)]
        public string Id { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "operation", IsRequired = false)]
        public FileOperationType OperationType { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "progress", IsRequired = false)]
        public int Progress { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "error", IsRequired = false)]
        public string Error { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "processed", IsRequired = false)]
        public string Processed { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "finished", IsRequired = false)]
        public bool Finished { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "url", IsRequired = false)]
        public string Url { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "files", IsRequired = true, EmitDefaultValue = true)]
        public List<FileWrapper> Files { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "folders", IsRequired = true, EmitDefaultValue = true)]
        public List<FolderWrapper> Folders { get; set; }

        /// <summary>
        /// </summary>
        public FileOperationWraper()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="o"></param>
        public FileOperationWraper(FileOperationResult o)
        {
            Id = o.Id;
            OperationType = o.OperationType;
            Progress = o.Progress;
            Error = o.Error;
            Processed = o.Processed;
            Finished = o.Finished;

            if (!string.IsNullOrEmpty(o.Result) && OperationType != FileOperationType.Delete)
            {
                var arr = o.Result.Split(':');
                var folders = arr.Where(s => s.StartsWith("folder_")).Select(s => s.Substring(7));
                if (folders.Any())
                {
                    using (var folderDao = Global.DaoFactory.GetFolderDao())
                    {
                        Folders = folderDao.GetFolders(folders.ToArray()).Select(r => new FolderWrapper(r)).ToList();
                    }
                }
                var files = arr.Where(s => s.StartsWith("file_")).Select(s => s.Substring(5));
                if (files.Any())
                {
                    using (var fileDao = Global.DaoFactory.GetFileDao())
                    {
                        Files = fileDao.GetFiles(files.ToArray()).Select(r => new FileWrapper(r)).ToList();
                    }
                }

                if (OperationType == FileOperationType.Download)
                {
                    Url = CommonLinkUtility.GetFullAbsolutePath(o.Result);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static FileOperationWraper GetSample()
        {
            return new FileOperationWraper
                {
                    Id = Guid.NewGuid().ToString(),
                    OperationType = FileOperationType.Move,
                    Progress = 100,
                    //Source = "folder_1,file_1",
                    //Result = "folder_1,file_1",
                    Error = "",
                    Processed = "1",
                    Files = new List<FileWrapper> { FileWrapper.GetSample() },
                    Folders = new List<FolderWrapper> { FolderWrapper.GetSample() }
                };
        }
    }
}