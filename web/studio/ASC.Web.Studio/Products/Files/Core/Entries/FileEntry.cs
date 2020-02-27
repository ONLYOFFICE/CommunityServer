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