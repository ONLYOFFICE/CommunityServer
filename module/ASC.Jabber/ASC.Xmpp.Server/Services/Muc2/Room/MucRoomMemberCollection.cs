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

using System.Linq;
using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;

namespace ASC.Xmpp.Server.Services.Muc2.Room
{
    using System;
    using System.Collections.Generic;
    using Handler;
    using Member;


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