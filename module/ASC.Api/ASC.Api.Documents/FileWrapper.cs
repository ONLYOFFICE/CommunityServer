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
using System.Globalization;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Core;
using ASC.Files.Core;
using ASC.Specific;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Utility;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;

namespace ASC.Api.Documents
{
    /// <summary>
    /// </summary>
    [DataContract(Name = "file", Namespace = "")]
    public class FileWrapper : FileEntryWrapper
    {
        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public object FolderId { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public int Version { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public int VersionGroup { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = true)]
        public String ContentLength { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = true)]
        public long PureContentLength { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public FileStatus FileStatus { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public String ViewUrl { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public String WebUrl { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public FileType FileType { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public String FileExst { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public String Comment { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool Encrypted { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="file"></param>
        public FileWrapper(File file)
            : base(file)
        {
            FolderId = file.FolderID;
            if (file.RootFolderType == FolderType.USER
                && !Equals(file.RootFolderCreator, SecurityContext.CurrentAccount.ID))
            {
                RootFolderType = FolderType.SHARE;
                using (var folderDao = Global.DaoFactory.GetFolderDao())
                {
                    var parentFolder = folderDao.GetFolder(file.FolderID);
                    if (!Global.GetFilesSecurity().CanRead(parentFolder))
                        FolderId = Global.FolderShare;
                }
            }

            FileExst = FileUtility.GetFileExtension(file.Title);
            FileType = FileUtility.GetFileTypeByExtention(FileExst);

            Version = file.Version;
            VersionGroup = file.VersionGroup;
            ContentLength = file.ContentLengthString;
            FileStatus = file.FileStatus;
            PureContentLength = file.ContentLength;
            Comment = file.Comment;
            Encrypted = file.Encrypted;
            try
            {
                ViewUrl = CommonLinkUtility.GetFullAbsolutePath(file.DownloadUrl);

                WebUrl = CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.GetFileWebPreviewUrl(file.Title, file.ID));
            }
            catch (Exception)
            {
                //Don't catch anything here because of httpcontext
            }
        }

        private FileWrapper()
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static FileWrapper GetSample()
        {
            return new FileWrapper
                {
                    Access = FileShare.ReadWrite,
                    Updated = ApiDateTime.GetSample(),
                    Created = ApiDateTime.GetSample(),
                    CreatedBy = EmployeeWraper.GetSample(),
                    Id = new Random().Next(),
                    RootFolderType = FolderType.BUNCH,
                    Shared = false,
                    Title = "Some titile.txt",
                    FileExst = ".txt",
                    FileType = FileType.Document,
                    UpdatedBy = EmployeeWraper.GetSample(),
                    ContentLength = "12.06 KB", //12345
                    PureContentLength = 12345,
                    FileStatus = FileStatus.IsNew,
                    FolderId = 12334,
                    Version = 3,
                    VersionGroup = 1,
                    ViewUrl = "http://www.onlyoffice.com/viewfile?fileid=2221"
                };
        }
    }
}