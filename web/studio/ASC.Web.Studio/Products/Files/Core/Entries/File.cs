/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Web.Core.Files;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace ASC.Files.Core
{
    [Flags]
    [DataContract(Namespace = "")]
    public enum FileStatus
    {
        [EnumMember] None = 0x0,

        [EnumMember] IsEditing = 0x1,

        [EnumMember] IsNew = 0x2,

        [EnumMember] IsConverting = 0x4,

        [EnumMember] IsOriginal = 0x8,

        [EnumMember] IsEditingAlone = 0x10
    }

    [Serializable]
    [DataContract(Name = "file", Namespace = "")]
    [DebuggerDisplay("{Title} ({ID} v{Version})")]
    public class File : FileEntry
    {
        private FileStatus _status;

        public File()
        {
            Version = 1;
            VersionGroup = 1;
            FileEntryType = FileEntryType.File;
        }

        public object FolderID { get; set; }

        [DataMember(Name = "version")]
        public int Version { get; set; }

        [DataMember(Name = "version_group")]
        public int VersionGroup { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "comment")]
        public string Comment { get; set; }

        public String PureTitle
        {
            get { return base.Title; }
            set { base.Title = value; }
        }

        [DataMember(Name = "title", IsRequired = true)]
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

        [DataMember(EmitDefaultValue = true, Name = "content_length", IsRequired = true)]
        public long ContentLength { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "content_length_string", IsRequired = true)]
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
                    case FileType.Archive:
                        return FilterType.ArchiveOnly;
                    case FileType.Audio:
                    case FileType.Video:
                        return FilterType.MediaOnly;
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

                if (FileTracker.IsEditingAlone(ID))
                {
                    _status |= FileStatus.IsEditingAlone;
                }

                if (FileConverter.IsConverting(this))
                {
                    _status |= FileStatus.IsConverting;
                }

                return _status;
            }
            set { _status = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "locked")]
        public bool Locked { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "locked_by")]
        public string LockedBy { get; set; }

        public override bool IsNew
        {
            get { return (_status & FileStatus.IsNew) == FileStatus.IsNew; }
            set
            {
                if(value)
                    _status |= FileStatus.IsNew;
                else
                    _status ^= FileStatus.IsNew;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "encrypted")]
        public bool Encrypted { get; set; }

        public ForcesaveType Forcesave { get; set; }

        public String DownloadUrl
        {
            get { return FilesLinkUtility.GetFileDownloadUrl(ID); }
        }

        public string ConvertedType { get; set; }

        public string ConvertedExtension
        {
            get
            {
                if (string.IsNullOrEmpty(ConvertedType)) return FileUtility.GetFileExtension(Title);

                var curFileType = FileUtility.GetFileTypeByFileName(Title);
                switch (curFileType)
                {
                    case FileType.Image:
                        return ConvertedType.Trim('.') == "zip" ? ".pptt" : ConvertedType;
                    case FileType.Spreadsheet:
                        return ConvertedType.Trim('.') != "xlsx" ? ".xlst" : ConvertedType;
                    case FileType.Document:
                        return ConvertedType.Trim('.') == "zip" ? ".doct" : ConvertedType;
                }
                return ConvertedType;
            }
        }

        public object NativeAccessor { get; set; }

        public static string Serialize(File file)
        {
            using (var ms = new FileEntrySerializer().ToXml(file))
            {
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}