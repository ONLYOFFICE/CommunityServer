/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASC.Core;
using ASC.ElasticSearch;
using ASC.ElasticSearch.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Core.Search
{
    public sealed class FilesWrapper : WrapperWithDoc
    {
        [Column("title", 1)]
        public string Title { get; set; }

        [ColumnLastModified("modified_on")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnMeta("version", 2)]
        public int Version { get; set; }

        [ColumnCondition("current_version", 3, true)]
        public bool Current { get; set; }

        [ColumnMeta("encrypted", 4)]
        public bool Encrypted { get; set; }

        [ColumnMeta("content_length", 5)]
        public long ContentLength { get; set; }

        [ColumnMeta("create_by", 6)]
        public Guid CreateBy { get; set; }

        [ColumnMeta("create_on", 7)]
        public DateTime CreateOn { get; set; }

        [ColumnMeta("category", 8)]
        public int Category { get; set; }


        [Join(JoinTypeEnum.Sub, "folder_id:folder_id")]
        public List<FilesFoldersWrapper> Folders { get; set; }

        protected override string Table { get { return "files_file"; } }

        public static FilesWrapper GetFilesWrapper(File d, List<object> parentFolders)
        {
            var wrapper = (FilesWrapper)d;
            wrapper.Folders = parentFolders.Select(r => new FilesFoldersWrapper() { FolderId = r.ToString() }).ToList();
            return wrapper;
        }

        public static implicit operator FilesWrapper(File d)
        {
            return new FilesWrapper
            {
                Id = (int)d.ID,
                Title = d.Title,
                Version = d.Version,
                Encrypted = d.Encrypted,
                ContentLength = d.ContentLength,
                LastModifiedOn = d.ModifiedOn,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
            };
        }

        public static explicit operator File(FilesWrapper d)
        {
            return new File
            {
                ID = d.Id,
                Title = d.Title,
                Version = d.Version,
                ContentLength = d.ContentLength
            };
        }

        protected override Stream GetDocumentStream()
        {
            CoreContext.TenantManager.SetCurrentTenant(TenantId);

            if (Encrypted) return null;
            if (!FileUtility.CanIndex(Title)) return null;

            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                var file = (File) this;

                if (!fileDao.IsExistOnStorage(file)) return null;
                if (file.ContentLength > MaxContentLength) return null;

                return fileDao.GetFileStream(file);
            }
        }

        public override string SettingsTitle
        {
            get { return FilesCommonResource.IndexTitle; }
        }
    }

    public sealed class FilesFoldersWrapper : Wrapper
    {
        [Column("parent_id", 1)]
        public string FolderId { get; set; }

        [ColumnId("")]
        public override int Id { get; set; }

        [ColumnTenantId("")]
        public override int TenantId { get; set; }

        [ColumnLastModified("")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table { get { return "files_folder_tree"; } }
    }
}