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


using System.Collections.Generic;
using System.Runtime.Serialization;
using ASC.Files.Core;

namespace ASC.Web.Files.Services.WCFService
{
    [DataContract(Name = "composite_data", Namespace = "")]
    public class DataWrapper
    {
        [DataMember(IsRequired = false, Name = "entries", EmitDefaultValue = false)]
        public ItemList<FileEntry> Entries { get; set; }

        [DataMember(IsRequired = false, Name = "total")]
        public int Total { get; set; }

        [DataMember(IsRequired = false, Name = "path_parts")]
        public ItemList<object> FolderPathParts { get; set; }

        [DataMember(IsRequired = false, Name = "folder_info")]
        public Folder FolderInfo { get; set; }

        [DataMember(IsRequired = false, Name = "root_folders_id_marked_as_new")]
        public Dictionary<object, int> RootFoldersIdMarkedAsNew { get; set; }
    }
}