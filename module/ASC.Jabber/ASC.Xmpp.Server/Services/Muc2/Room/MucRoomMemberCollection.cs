/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Xmpp.Server.Services.Muc2.Room.Member;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Xmpp.Server.Services.Muc2.Room
{
    internal class MucRoomMemberCollection : List<MucRoomMember>
    {
        private readonly XmppServiceManager manager;

        public MucRoomMemberCollection(XmppServiceManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            this.manager = manager;
        }

        internal event MemberActionDelegate MemberAdded = null;

        private void InvokeMemberAdded(MucRoomMember member)
        {
            MemberActionDelegate evt = MemberAdded;
            if (evt != null)
            {
                evt(member);
            }
        }

        internal event MemberActionDelegate MemberRemoved = null;

        private void InvokeMemberRemoved(MucRoomMember member)
        {
            MemberActionDelegate evt = MemberRemoved;
            if (evt != null)
            {
                evt(member);
            }
        }

        internal new void Add(MucRoomMember member)
        {
            if (!Contains(member))
            {
                base.Add(member);
                manager.RegisterService(member);
                InvokeMemberAdded(member);
            }
            else
            {
                throw new Exceptions.MucMemberExistsException();
            }
        }

        internal new void Remove(MucRoomMember member)
        {
            if (Contains(member))
            {
                base.Remove(member);
                InvokeMemberRemoved(member);
                manager.UnregisterService(member.Jid);
            }
        }

        internal new void Clear()
        {
            foreach (MucRoomMember member in this)
            {
                manager.UnregisterService(member.Jid);
            }
            base.Clear();
        }

        internal MucRoomMember FindByJid(Jid jid)
        {
            return Find((x) => x.Jid.Equals(jid));
        }

        internal MucRoomMember FindByRealJid(Jid jid)
        {
            return Find((x) => jid.Equals(x.RealJid));
        }

        internal MucRoomMember this[string jid]
        {
            get
            {
                return FindByJid(new Jid(jid));
            }
        }
        internal MucRoomMember this[Jid jid]
        {
            get
            {
                return FindByJid(jid);
            }
        }

        public void RebindAddress(MucRoomMember member, Jid address)
        {
            manager.UnregisterService(member.Jid);
            member.Jid = address;
            manager.RegisterService(member);
        }

        public MucRoomMember FindByNick(string nickname)
        {
            foreach (MucRoomMember member in this)
            {
                if (member != null && member.Nick != null && member.Nick.Equals(nickname, StringComparison.OrdinalIgnoreCase))
                {
                    return member;
                }
            }
            return null;
        }

        public MucRoomMember FindByBareJid(Jid jidBare)
        {
            return this.Where(member => member != null && member.RealJid != null && member.RealJid.Bare.Equals(jidBare.Bare, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
        }
    }
}