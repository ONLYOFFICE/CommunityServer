/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
        public bool SharedByMe { get; set; }

        [DataMember(Name = "provider_id", EmitDefaultValue = false)]
        public int ProviderId { get; set; }

        [DataMember(Name = "provider_key", EmitDefaultValue = false)]
        public string ProviderKey { get; set; }

        public bool ProviderEntry
        {
            get { return !string.IsNullOrEmpty(ProviderKey); }
        }

        public DateTime? SharedToMeOn { get; set; }

        public String SharedToMeBy { get; set; }

        public DateTime CreateOn { get; set; }

        public virtual Guid ModifiedBy { get; set; }

        public DateTime ModifiedOn { get; set; }

        public FolderType RootFolderType { get; set; }

        public Guid RootFolderCreator { get; set; }

        public object RootFolderId { get; set; }

        public abstract bool IsNew { get; set; }

        public String UniqID
        {
            get { return String.Format("{0}_{1}", GetType().Name.ToLower(), ID); }
        }

        private string _modifiedByString;
        private string _createByString;

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