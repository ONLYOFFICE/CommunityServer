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
