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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Configuration;
using ASC.Files.Core.Security;
using ASC.Web.Core.Files;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;

namespace ASC.Files.Core
{
    [Flags]
    [DataContract]
    public enum FileStatus
    {
        [EnumMember] None = 0x0,

        [EnumMember] IsEditing = 0x1,

        [EnumMember] IsNew = 0x2,

        [EnumMember] IsConverting = 0x4,

        [EnumMember] IsOriginal = 0x8
    }

    [DataContract(Name = "file", Namespace = "")]
    [DebuggerDisplay("{Title} ({ID} v{Version})")]
    public class File : FileEntry
    {
        private FileStatus _status;

        public File()
        {
            Version = 1;
            VersionGroup = 1;
        }

        public object FolderID { get; set; }

        [DataMember(EmitDefaultValue = true, Name = "version", IsRequired = false)]
        public int Version { get; set; }

        [DataMember(EmitDefaultValue = true, Name = "version_group", IsRequired = false)]
        public int VersionGroup { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "comment")]
        public string Comment { get; set; }

        public String PureTitle
        {
            get { return base.Title; }
            set { base.Title = value; }
        }

        public override string Title
        {
            get
            {
                return String.IsNullOrEmpty(ConvertedType)
                           ? base.Title
                           : FileUtility.ReplaceFileExtension(base.Title, FileUtility.GetInternalExtension(base.Title));
            }
            set { base.Title = value; }
        }

        public long ContentLength { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "content_length", IsRequired = true)]
        public String ContentLengthString
        {
            get { return FileSizeComment.FilesSizeToString(ContentLength); }
            set { }
        }

        public FilterType FilterType
        {
            get
            {
                switch (FileUtility.GetFileTypeByFileName(Title))
                {
                    case FileType.Image:
                        return FilterType.ImagesOnly;
                    case FileType.Document:
                        return FilterType.DocumentsOnly;
                    case FileType.Presentation:
                        return FilterType.PresentationsOnly;
                    case FileType.Spreadsheet:
                        return FilterType.SpreadsheetsOnly;
                }

                return FilterType.None;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "file_status")]
        public FileStatus FileStatus
        {
            get
            {
                if (FileTracker.IsEditing(ID))
                {
                    _status |= FileStatus.IsEditing;
                }

                if (FileConverter.IsConverting(this))
                {
                    _status |= FileStatus.IsConverting;
                }

                DateTime requiredDate;
                if (
                    DateTime.TryParse(WebConfigurationManager.AppSettings["files.mark-original.date"], out requiredDate)
                    && ModifiedOn.Date < requiredDate
                    && String.IsNullOrEmpty(ConvertedType)
                    && !ProviderEntry
                    && new List<string> { ".docx", ".xlsx", ".pptx" }.Contains(FileUtility.GetFileExtension(Title))
                    )
                {
                    _status |= FileStatus.IsOriginal;
                }

                return _status;
            }
            set { _status = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "locked")]
        public bool Locked { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "locked_by")]
        public string LockedBy { get; set; }

        public String ThumbnailURL { get; set; }

        public String FileDownloadUrl
        {
            get { return FilesLinkUtility.GetFileDownloadUrl(ID); }
        }

        public String ViewUrl
        {
            get { return FilesLinkUtility.GetFileViewUrl(ID); }
        }

        public string ConvertedType { get; set; }

        public string ConvertedExtension
        {
            get
            {
                if (string.IsNullOrEmpty(ConvertedType)) return FileUtility.GetFileExtension(Title);

                //hack: Use only for old internal format

                var curFileType = FileUtility.GetFileTypeByFileName(Title);
                switch (curFileType)
                {
                    case FileType.Image:
                        return ConvertedType == ".zip" ? ".pptt" : ConvertedType;
                    case FileType.Spreadsheet:
                        return ConvertedType != ".xlsx" ? ".xlst" : ConvertedType;
                }
                return ConvertedType;
            }
        }

        public object NativeAccessor { get; set; }

        public static string Serialize(File file)
        {
            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(File));
                serializer.WriteObject(ms, file);
                ms.Seek(0, SeekOrigin.Begin);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }
    }

    public class SharedFile : File
    {
        public SharedFile()
        {
            Shares = new List<SmallShareRecord>();
        }

        public override Guid CreateBy { get; set; }

        public override Guid ModifiedBy { get; set; }

        public List<SmallShareRecord> Shares { get; set; }
    }
}