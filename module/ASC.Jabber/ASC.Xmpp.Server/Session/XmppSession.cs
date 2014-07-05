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
using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Utils;
using System.ComponentModel;

namespace ASC.Xmpp.Server.Session
{
	public class XmppSession
	{
		public string Id
		{
			get;
			private set;
		}

		public Jid Jid
		{
			get;
			private set;
		}

		public bool Active
		{
			get;
			set;
		}

		public XmppStream Stream
		{
			get;
			private set;
		}

		public bool RosterRequested
		{
			get;
			set;
		}

		public int Priority
		{
			get;
			set;
		}

		public Presence Presence
		{
			get { return presence; }
			set
			{
				presence = value;
				Priority = presence != null ? presence.Priority : 0;
			}
		}

		private Presence presence;

		public bool Available
		{
			get { return Presence != null && (Presence.Type == PresenceType.available || Presence.Type == PresenceType.invisible); }
		}

		public ClientInfo ClientInfo
		{
			get;
			private set;
		}

        public DateTime GetRosterTime
        {
            get;
            set;
        }

        [DefaultValue(false)]
        public bool IsSignalRFake
        {
            get;
            set;
        }

		public XmppSession(Jid jid, XmppStream stream)
		{
			if (jid == null) throw new ArgumentNullException("jid");
			if (stream == null) throw new ArgumentNullException("stream");

			Id = UniqueId.CreateNewId();
			Jid = jid;
			Stream = stream;
			Active = false;
			RosterRequested = false;
			ClientInfo = new ClientInfo();
		}

        public override string ToString()
        {
            return Jid.ToString();
        }
	}
}
