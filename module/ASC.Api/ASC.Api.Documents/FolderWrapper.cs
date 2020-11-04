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
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Specific;
using ASC.Web.Files.Classes;

namespace ASC.Api.Documents
{
    /// <summary>
    /// </summary>
    [DataContract(Name = "folder", Namespace = "")]
    public class FolderWrapper : FileEntryWrapper
    {
        /// <summary>
        /// </summary>
        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public object ParentId { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public int FilesCount { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public int FoldersCount { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool IsShareable { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="folder"></param>
        public FolderWrapper(Folder folder)
            : base(folder)
        {
            ParentId = folder.ParentFolderID;
            if (folder.RootFolderType == FolderType.USER
                && !Equals(folder.RootFolderCreator, SecurityContext.CurrentAccount.ID))
            {
                RootFolderType = FolderType.SHARE;

                using (var folderDao = Global.DaoFactory.GetFolderDao())
                {
                    var parentFolder = folderDao.GetFolder(folder.ParentFolderID);
                    if (!Global.GetFilesSecurity().CanRead(parentFolder))
                        ParentId = Global.FolderShare;
                }
            }

            FilesCount = folder.TotalFiles;
            FoldersCount = folder.TotalSubFolders;
            IsShareable = folder.Shareable;
        }

        private FolderWrapper()
            : base()
        {

        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static FolderWrapper GetSample()
        {
            return new FolderWrapper
                {
                    Access = FileShare.ReadWrite,
                    Updated = ApiDateTime.GetSample(),
                    Created = ApiDateTime.GetSample(),
                    CreatedBy = EmployeeWraper.GetSample(),
                    Id = new Random().Next(),
                    RootFolderType = FolderType.BUNCH,
                    Shared = false,
                    Title = "Some titile",
                    UpdatedBy = EmployeeWraper.GetSample(),
                    FilesCount = new Random().Next(),
                    FoldersCount = new Random().Next(),
                    ParentId = new Random().Next(),
                    IsShareable = false
                };
        }
    }
}