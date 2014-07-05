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
using System.Runtime.Serialization;
using ASC.Files.Core.Security;

namespace ASC.Files.Core
{
    [DataContract]
    public enum FolderType
    {
        [EnumMember] DEFAULT = 0,

        [EnumMember] COMMON = 1,

        [EnumMember] BUNCH = 2,

        [EnumMember] TRASH = 3,

        [EnumMember] USER = 5,

        [EnumMember] SHARE = 6,

        [EnumMember] Projects = 8
    }

    [DataContract(Name = "folder", Namespace = "")]
    [DebuggerDisplay("{Title} ({ID})")]
    public class Folder : FileEntry
    {
        public FolderType FolderType { get; set; }

        public object ParentFolderID { get; set; }

        [DataMember(Name = "total_files", EmitDefaultValue = true, IsRequired = false)]
        public int TotalFiles { get; set; }

        [DataMember(Name = "total_sub_folder", EmitDefaultValue = true, IsRequired = false)]
        public int TotalSubFolders { get; set; }

        [DataMember(Name = "shareable")]
        public bool Shareable { get; set; }

        [DataMember(Name = "isnew")]
        public int NewForMe { get; set; }

        [DataMember(Name = "folder_url", EmitDefaultValue = false)]
        public string FolderUrl { get; set; }

        public Folder()
        {
            Title = String.Empty;
        }
    }

    public class SharedFolder : Folder
    {
        public SharedFolder()
        {
            Shares = new List<SmallShareRecord>();
        }

        public override Guid CreateBy { get; set; }

        public override Guid ModifiedBy { get; set; }

        public List<SmallShareRecord> Shares { get; set; }
    }
}