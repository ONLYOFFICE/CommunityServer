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

        [EnumMember] IsEditingAlone = 0x10,

        [EnumMember] IsFavorite = 0x20,

        [EnumMember] IsTemplate = 0x40
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
                if (value)
                    _status |= FileStatus.IsNew;
                else
                    _status ^= FileStatus.IsNew;
            }
        }

        public bool IsFavorite
        {
            get { return (_status & FileStatus.IsFavorite) == FileStatus.IsFavorite; }
            set
            {
                if (value)
                    _status |= FileStatus.IsFavorite;
                else
                    _status ^= FileStatus.IsFavorite;
            }
        }

        public bool IsTemplate
        {
            get { return (_status & FileStatus.IsTemplate) == FileStatus.IsTemplate; }
            set
            {
                if (value)
                    _status |= FileStatus.IsTemplate;
                else
                    _status ^= FileStatus.IsTemplate;
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