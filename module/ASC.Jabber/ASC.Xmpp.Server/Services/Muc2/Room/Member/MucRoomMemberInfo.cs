/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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