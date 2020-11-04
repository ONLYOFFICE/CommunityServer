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
using ASC.Files.Core.Security;
using ASC.Web.Files.Classes;

namespace ASC.Files.Core
{
    [DataContract(Name = "entry", Namespace = "")]
    [KnownType(typeof (Folder))]
    [KnownType(typeof (File))]
    [Serializable]
    public abstract class FileEntry : ICloneable
    {
        [DataMember(Name = "id")]
        public object ID { get; set; }

        [DataMember(Name = "title", IsRequired = true)]
        public virtual string Title { get; set; }

        [DataMember(Name = "create_by_id")]
        public Guid CreateBy { get; set; }

        [DataMember(Name = "create_by")]
        public string CreateByString
        {
            get { return !CreateBy.Equals(Guid.Empty) ? Global.GetUserName(CreateBy) : _createByString; }
            set { _createByString = value; }
        }

        [DataMember(Name = "create_on")]
        public string CreateOnString
        {
            get { return CreateOn.Equals(default(DateTime)) ? null : CreateOn.ToString("g"); }
            set { throw new NotImplementedException(); }
        }

        [DataMember(Name = "modified_on")]
        public string ModifiedOnString
        {
            get { return ModifiedOn.Equals(default(DateTime)) ? null : ModifiedOn.ToString("g"); }
            set { throw new NotImplementedException(); }
        }

        [DataMember(Name = "modified_by_id")]
        public Guid ModifiedBy { get; set; }

        [DataMember(Name = "modified_by")]
        public string ModifiedByString
        {
            get { return !ModifiedBy.Equals(Guid.Empty) ? Global.GetUserName(ModifiedBy) : _modifiedByString; }
            set { _modifiedByString = value; }
        }

        [DataMember(Name = "error", EmitDefaultValue = false)]
        public string Error { get; set; }

        [DataMember(Name = "access")]
        public FileShare Access { get; set; }

        [DataMember(Name = "shared")]
        public bool Shared { get; set; }

        [DataMember(Name = "provider_id", EmitDefaultValue = false)]
        public int ProviderId { get; set; }

        [DataMember(Name = "provider_key", EmitDefaultValue = false)]
        public string ProviderKey { get; set; }

        [DataMember(Name = "folder_id")]
        public object FolderIdDisplay
        {
            get
            {
                if (_folderIdDisplay != null) return _folderIdDisplay;

                var folder = this as Folder;
                if (folder != null) return folder.ParentFolderID;

                var file = this as File;
                if (file != null) return file.FolderID;

                return null;
            }
            set { _folderIdDisplay = value; }
        }

        public bool ProviderEntry
        {
            get { return !string.IsNullOrEmpty(ProviderKey); }
        }

        public DateTime CreateOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public FolderType RootFolderType { get; set; }

        public Guid RootFolderCreator { get; set; }

        public object RootFolderId { get; set; }

        public abstract bool IsNew { get; set; }

        public FileEntryType FileEntryType;

        public String UniqID
        {
            get { return String.Format("{0}_{1}", GetType().Name.ToLower(), ID); }
        }

        private string _modifiedByString;
        private string _createByString;
        private object _folderIdDisplay;

        public override bool Equals(object obj)
        {
            var f = obj as FileEntry;
            return f != null && Equals(f.ID, ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return Title;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}