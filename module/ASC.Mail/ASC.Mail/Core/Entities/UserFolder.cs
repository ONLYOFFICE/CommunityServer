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

namespace ASC.Mail.Core.Entities
{
    public class UserFolder : IEquatable<UserFolder>
    {
        public uint Id { get; set; }
        public uint ParentId { get; set; }
        public string User { get; set; }
        public int Tenant { get; set; }
        public string Name { get; set; }
        public int FolderCount { get; set; }
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
        public int UnreadChainCount { get; set; }
        public int TotalChainCount { get; set; }
        public DateTime TimeModified { get; set; }

        public bool Equals(UserFolder other)
        {
            if (other == null) return false;
            return string.Equals(User, other.User, StringComparison.InvariantCultureIgnoreCase)
                   && Tenant == other.Tenant
                   && Id == other.Id
                   && ParentId == other.ParentId
                   && string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase)
                   && FolderCount == other.FolderCount
                   && UnreadCount == other.UnreadCount
                   && TotalCount == other.TotalCount
                   && UnreadChainCount == other.UnreadChainCount
                   && TotalChainCount == other.TotalChainCount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as UserFolder);
        }

        public override int GetHashCode()
        {
            return Tenant.GetHashCode()
                   ^ User.GetHashCode()
                   ^ Id.GetHashCode()
                   ^ ParentId.GetHashCode()
                   ^ Name.GetHashCode()
                   ^ FolderCount.GetHashCode()
                   ^ UnreadCount.GetHashCode()
                   ^ TotalCount.GetHashCode()
                   ^ UnreadChainCount.GetHashCode()
                   ^ TotalChainCount.GetHashCode();
        }
    }
}
