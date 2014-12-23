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

using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.x.muc;

namespace ASC.Xmpp.Server.Services.Muc2.Room.Member
{
    using System;

    public class MucRoomMemberInfo : IEquatable<MucRoomMemberInfo>
    {
        public Jid Jid { get; set; }
        public Affiliation Affiliation { get; set;}
        public Role Role { get; set;}

        public MucRoomMemberInfo()
        {
            
        }

        public MucRoomMemberInfo(string record)
        {
            string[] fields = record.Trim(';',' ').Split(':');
            if (fields.Length!=3)
            {
                throw new ArgumentException("bad format");
            }
            Jid = new Jid(fields[0]);
            Affiliation = (Affiliation) Enum.Parse(typeof (Affiliation), fields[1]);
            Role = (Role)Enum.Parse(typeof(Role), fields[2]);
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", Jid.Bare, Affiliation, Role);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public bool Equals(MucRoomMemberInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(other.Jid.Bare, Jid.Bare);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return (Jid != null ? Jid.GetHashCode() : 0);
        }

        public static bool operator ==(MucRoomMemberInfo left, MucRoomMemberInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MucRoomMemberInfo left, MucRoomMemberInfo right)
        {
            return !Equals(left, right);
        }
    }
}