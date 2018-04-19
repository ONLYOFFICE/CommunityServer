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
                    ContentLength = 12345.ToString(CultureInfo.InvariantCulture),
                    FileStatus = FileStatus.IsNew,
                    FolderId = 12334,
                    Version = 3,
                    VersionGroup = 1,
                    ViewUrl = "http://www.onlyoffice.com/viewfile?fileid=2221"
                };
        }
    }
}