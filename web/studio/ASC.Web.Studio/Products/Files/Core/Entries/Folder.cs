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
using System.Diagnostics;
using System.Runtime.Serialization;

namespace ASC.Files.Core
{
    [DataContract(Namespace = "")]
    public enum FolderType
    {
        [EnumMember] DEFAULT = 0,

        [EnumMember] COMMON = 1,

        [EnumMember] BUNCH = 2,

        [EnumMember] TRASH = 3,

        [EnumMember] USER = 5,

        [EnumMember] SHARE = 6,

        [EnumMember] Projects = 8,

        [EnumMember] Favorites = 10,

        [EnumMember] Recent = 11,

        [EnumMember] Templates = 12,

        [EnumMember] Privacy = 13,
    }

    [DataContract(Name = "folder", Namespace = "")]
    [DebuggerDisplay("{Title} ({ID})")]
    public class Folder : FileEntry
    {
        public FolderType FolderType { get; set; }

        public object ParentFolderID { get; set; }

        [DataMember(Name = "total_files")]
        public int TotalFiles { get; set; }

        [DataMember(Name = "total_sub_folder")]
        public int TotalSubFolders { get; set; }

        [DataMember(Name = "shareable", EmitDefaultValue = false)]
        public bool Shareable { get; set; }

        [DataMember(Name = "isnew")]
        public int NewForMe { get; set; }

        [DataMember(Name = "folder_url", EmitDefaultValue = false)]
        public string FolderUrl { get; set; }

        public override bool IsNew
        {
            get { return Convert.ToBoolean(NewForMe); }
            set { NewForMe = Convert.ToInt32(value); }
        }

        public Folder()
        {
            Title = String.Empty;
            FileEntryType = FileEntryType.Folder;
        }
    }
}