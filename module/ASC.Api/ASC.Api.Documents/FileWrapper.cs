/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.IO;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Core;
using ASC.Files.Core;
using ASC.Specific;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
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
            try
            {
                ViewUrl = file.ViewUrl;

                WebUrl = FilesLinkUtility.GetFileWebPreviewUrl(file.Title, file.ID);
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
                    Updated = (ApiDateTime)DateTime.UtcNow,
                    Created = (ApiDateTime)DateTime.UtcNow,
                    CreatedBy = EmployeeWraper.GetSample(),
                    Id = new Random().Next(),
                    RootFolderType = FolderType.BUNCH,
                    SharedByMe = false,
                    Title = "Some titile.txt",
                    FileExst = ".txt",
                    FileType = FileType.Document,
                    UpdatedBy = EmployeeWraper.GetSample(),
                    ContentLength = 12345.ToString(),
                    FileStatus = FileStatus.IsNew,
                    FolderId = 12334,
                    Version = 3,
                    VersionGroup = 1,
                    ViewUrl = "http://www.teamlab.com/viewfile?fileid=2221"
                };
        }
    }
}