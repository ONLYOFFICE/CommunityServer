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

using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.roster;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Storage.Interface;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Utils;
using System;

namespace ASC.Xmpp.Server.Services.Jabber
{
	[XmppHandler(typeof(Roster))]
	class RosterHandler : XmppStanzaHandler
	{
		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.HasTo && iq.To != iq.From) return XmppStanzaError.ToForbidden(iq);

			if (iq.Type == IqType.get) return GetRoster(stream, iq, context);
			else if (iq.Type == IqType.set) return SetRoster(stream, iq, context);
			else return null;
		}

		private IQ GetRoster(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.To = iq.From;
			var roster = new Roster();
			answer.Query = roster;

			foreach (var item in context.StorageManager.RosterStorage.GetRosterItems(iq.From))
			{
				roster.AddRosterItem(item.ToRosterItem());
			}
			var session = context.SessionManager.GetSession(iq.From);
            session.RosterRequested = true;
            session.GetRosterTime = DateTime.UtcNow;
			return answer;
		}

		private IQ SetRoster(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.To = iq.From;
			answer.From = iq.To;

			iq.Id = UniqueId.CreateNewId();
			var roster = (Roster)iq.Query;
			UserRosterItem item = null;
			try
			{
				var rosterItems = roster.GetRoster();
				if (rosterItems.Length != 1) throw new JabberException(ErrorCode.BadRequest);

				var rosterItem = rosterItems[0];
				item = UserRosterItem.FromRosterItem(rosterItem);

				if (rosterItem.Subscription == SubscriptionType.remove)
				{
					context.StorageManager.RosterStorage.RemoveRosterItem(iq.From, item.Jid);

					//Send presences
					var unsubscribe = new Presence() { Type = PresenceType.unsubscribe, To = item.Jid, From = iq.From };
					var unsubscribed = new Presence() { Type = PresenceType.unsubscribed, To = item.Jid, From = iq.From };
					var unavailable = new Presence() { Type = PresenceType.unavailable, To = item.Jid, From = iq.From };

					bool sended = false;
					foreach (var session in context.SessionManager.GetBareJidSessions(item.Jid))
					{
						if (session.RosterRequested)
						{
							context.Sender.SendTo(session, unsubscribe);
							context.Sender.SendTo(session, unsubscribed);
							sended = true;
						}
						context.Sender.SendTo(session, unavailable);
					}
					if (!sended)
					{
						context.StorageManager.OfflineStorage.SaveOfflinePresence(unsubscribe);
						context.StorageManager.OfflineStorage.SaveOfflinePresence(unsubscribed);
					}
				}
				else
				{
					item = context.StorageManager.RosterStorage.SaveRosterItem(iq.From, item);
					roster.RemoveAllChildNodes();
					roster.AddRosterItem(item.ToRosterItem());
				}
				//send all available user's resources
				context.Sender.Broadcast(context.SessionManager.GetBareJidSessions(iq.From), iq);
			}
			catch (System.Exception)
			{
				roster.RemoveAllChildNodes();
				item = context.StorageManager.RosterStorage.GetRosterItem(iq.From, item.Jid);
				if (item != null)
				{
					roster.AddRosterItem(item.ToRosterItem());
					context.Sender.Broadcast(context.SessionManager.GetBareJidSessions(iq.From), iq);
				}
				throw;
			}

			return answer;
		}
	}
}