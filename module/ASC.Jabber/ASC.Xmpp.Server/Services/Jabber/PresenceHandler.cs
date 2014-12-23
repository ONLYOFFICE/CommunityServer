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
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.roster;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Streams;
using System;
using System.Diagnostics;

namespace ASC.Xmpp.Server.Services.Jabber
{
	[XmppHandler(typeof(Presence))]
	class PresenceHandler : XmppStanzaHandler
	{
		private StorageManager storages;

		private IXmppSender sender;

		private XmppSessionManager sessionManager;

		public override void OnRegister(IServiceProvider serviceProvider)
		{
			storages = (StorageManager)serviceProvider.GetService(typeof(StorageManager));
			sender = (IXmppSender)serviceProvider.GetService(typeof(IXmppSender));
			sessionManager = (XmppSessionManager)serviceProvider.GetService(typeof(XmppSessionManager));
		}

		public override void HandlePresence(XmppStream stream, Presence presence, XmppHandlerContext context)
		{
			var session = sessionManager.GetSession(presence.From);
			if (session == null)
			{
				context.Sender.SendTo(stream, XmppStanzaError.ToNotFound(presence));
				return;
			}

			if (presence.Type == PresenceType.available || presence.Type == PresenceType.invisible || presence.Type == PresenceType.unavailable)
			{
				sessionManager.SetSessionPresence(session, presence);
			}

			if (presence.HasTo && presence.To.ToString() != stream.Domain)
			{
				HandlePresence(presence, session);//Presence to one of contacts
			}
			else
			{
				BroadcastPresence(presence);
			}
		}

		public void BroadcastPresence(Presence presence)
		{
			Debug.Assert(IsClientPresence(presence), "Can't set non client presence to server!!!");
			//User send presence directly to server
			if (IsClientPresence(presence) && !string.IsNullOrEmpty(presence.From.User))
			{
				foreach (var ri in storages.RosterStorage.GetRosterItems(presence.From))
				{
					if (ri.Subscribtion == SubscriptionType.from || ri.Subscribtion == SubscriptionType.both)
					{
                        sender.Broadcast(sessionManager.GetBareJidSessions(ri.Jid), presence);
					}
				}
                sender.SendPresenceToSignalR(presence, sessionManager);
			}
		}

		private bool IsClientPresence(Presence presence)
		{
			return presence.Type == PresenceType.available || presence.Type == PresenceType.invisible || presence.Type == PresenceType.probe || presence.Type == PresenceType.unavailable;
		}

		private void HandlePresence(Presence presence, XmppSession session)
		{
			var toRosterItem = GetUserRosterItem(presence.From, presence.To);
			var fromRosterItem = GetUserRosterItem(presence.To, presence.From);

			var stateOut = GetState(fromRosterItem, toRosterItem);
			var stateIn = GetState(toRosterItem, fromRosterItem);

			bool bRoute = false;
			bool bAutoReply = false;
			bool bMutualCreating =
				(fromRosterItem.Subscribtion == SubscriptionType.to && toRosterItem.Subscribtion == SubscriptionType.from) ||
				(fromRosterItem.Subscribtion == SubscriptionType.from && toRosterItem.Subscribtion == SubscriptionType.to);

			var newType = presence.Type;
			if (newType == PresenceType.subscribe)
			{
				if (bMutualCreating)
				{
					if (toRosterItem.Subscribtion == SubscriptionType.from && toRosterItem.Ask == AskType.NONE)
					{
						//Push roster with ASK=subscribe
						toRosterItem.Ask = AskType.subscribe;
						UpdateItems(presence, ref fromRosterItem, ref toRosterItem);
						//Push
						sender.Broadcast(
							sessionManager.GetBareJidSessions(presence.From),
							toRosterItem.GetRosterIq(presence.From)
						);
						//Forward
						presence.To = new Jid(presence.To.Bare);
						bool sended = sender.Broadcast(sessionManager.GetBareJidSessions(presence.To), presence);

						if (!sended) StoreOffline(presence);
					}
				}
				else
				{
					//it's inbound for user 'to'
					ChangeInboundPresence(stateIn, toRosterItem, fromRosterItem, newType, out bRoute, out bAutoReply);
					if (bAutoReply)
					{
						//Reply with 'subscribed'
						var autoPresence = new Presence();
						autoPresence.To = presence.From;
						autoPresence.From = presence.To;
						autoPresence.Type = PresenceType.subscribed;
						sender.Broadcast(sessionManager.GetBareJidSessions(autoPresence.To), autoPresence);
					}
					else
					{
						if (bRoute)
						{
							//Send to 'to' user
							presence.To = new Jid(presence.To.Bare);
							bool sended = sender.Broadcast(sessionManager.GetBareJidSessions(presence.To), presence);
							if (!sended) StoreOffline(presence);

							//State is changed init roster push
							UpdateItems(presence, ref fromRosterItem, ref toRosterItem);
							sender.Broadcast(
								sessionManager.GetBareJidSessions(presence.From),
								toRosterItem.GetRosterIq(presence.From)
							);
							//Send result stanza
							sender.SendTo(session, new IQ(IqType.result));
						}
					}
				}
			}
			else if (newType == PresenceType.subscribed)
			{
				if (bMutualCreating)
				{
					if (fromRosterItem.Subscribtion == SubscriptionType.from && fromRosterItem.Ask == AskType.subscribe)
					{
						//Send roster to contact with both to 'from'
						//Create both subscription

						//Send subscribed back
						toRosterItem.Subscribtion = SubscriptionType.both;
						toRosterItem.Ask = AskType.NONE;
						UpdateItems(presence, ref fromRosterItem, ref toRosterItem);
						sender.Broadcast(
							sessionManager.GetBareJidSessions(presence.From),
							toRosterItem.GetRosterIq(presence.From)
						);
						sender.SendTo(session, new IQ(IqType.result));
						//fwd
						presence.To = new Jid(presence.To.Bare);
						if (!sender.Broadcast(sessionManager.GetBareJidSessions(presence.To), presence))
						{
							StoreOffline(presence);
						}
						//Send contact with both
						fromRosterItem.Subscribtion = SubscriptionType.both;
						fromRosterItem.Ask = AskType.NONE;
						UpdateItems(presence, ref fromRosterItem, ref toRosterItem);
						sender.Broadcast(
							sessionManager.GetBareJidSessions(presence.To),
							fromRosterItem.GetRosterIq(presence.To)
						);
						//Send to session presence
						if (session.Presence != null)
						{
							sender.Broadcast(sessionManager.GetBareJidSessions(presence.To), session.Presence);
						}
					}
				}
				else
				{
					//It's outbound for user 'from'
					ChangeOutboundPresence(stateOut, toRosterItem, fromRosterItem, newType, out bRoute);
					//Roster oush for 'from'
					if (bRoute)
					{
						//State is changed init roster push
						UpdateItems(presence, ref fromRosterItem, ref toRosterItem);
						sender.Broadcast(
							sessionManager.GetBareJidSessions(presence.From),
							toRosterItem.GetRosterIq(presence.From)
						);
						//Send result stanza
						sender.SendTo(session, new IQ(IqType.result)); //dont send
					}
					//It's inbound for user 'to'
					if ((fromRosterItem.Subscribtion == SubscriptionType.none || fromRosterItem.Subscribtion == SubscriptionType.from) &&
						fromRosterItem.Ask == AskType.subscribe)
					{
						ChangeInboundPresence(stateIn, fromRosterItem, toRosterItem, newType, out bRoute, out bAutoReply);
						if (bRoute)
						{
							presence.To = new Jid(presence.To.Bare);
							if (sender.Broadcast(sessionManager.GetBareJidSessions(presence.To), presence))
							{
								StoreOffline(presence);
							}
							else
							{
								if (session.Presence != null)
								{
									sender.Broadcast(sessionManager.GetBareJidSessions(presence.To), session.Presence);
								}
							}
							//State is changed init roster push
							UpdateItems(presence, ref fromRosterItem, ref toRosterItem);
							sender.Broadcast(
								sessionManager.GetBareJidSessions(presence.To),
								fromRosterItem.GetRosterIq(presence.To));
						}
					}
				}
			}
			else if (newType == PresenceType.unsubscribe)
			{
				//Get to roster
				//it's inbound for user 'to'
				ChangeInboundPresence(stateIn, toRosterItem, fromRosterItem, newType, out bRoute, out bAutoReply);
				if (bAutoReply)
				{
					//Reply with 'subscribed'
					Presence autoPresence = new Presence();
					autoPresence.To = presence.From;
					autoPresence.From = presence.To;
					autoPresence.Type = PresenceType.unsubscribed;
					sender.Broadcast(sessionManager.GetBareJidSessions(autoPresence.To), autoPresence);
					//Route
					presence.To = new Jid(presence.To.Bare);
					if (!sender.Broadcast(sessionManager.GetBareJidSessions(presence.To), presence))
					{
						StoreOffline(presence);
					}
				}
			}
			else if (newType == PresenceType.unsubscribed)
			{


				//It's outbound for user 'from'
				ChangeOutboundPresence(stateOut, toRosterItem, fromRosterItem, newType, out bRoute);
				//Roster oush for 'from'
				if (bRoute)
				{
					//State is changed init roster push
					UpdateItems(presence, ref fromRosterItem, ref toRosterItem);
					sender.Broadcast(
						sessionManager.GetBareJidSessions(presence.From),
						toRosterItem.GetRosterIq(presence.From));
					//Send result stanza
					sender.SendTo(session, new IQ(IqType.result)); //dont send
				}

				ChangeInboundPresence(stateIn, fromRosterItem, toRosterItem, newType, out bRoute, out bAutoReply);
				if (bRoute)
				{
					presence.To = new Jid(presence.To.Bare);
					if (!sender.Broadcast(sessionManager.GetBareJidSessions(presence.To), presence))
					{
						StoreOffline(presence);
					}
					else
					{
						sender.Broadcast(
							sessionManager.GetBareJidSessions(presence.To),
							new Presence() { Type = PresenceType.unavailable, From = presence.From });
					}
					//State is changed init roster push
					UpdateItems(presence, ref fromRosterItem, ref toRosterItem);
					sender.Broadcast(
						sessionManager.GetBareJidSessions(presence.To),
						fromRosterItem.GetRosterIq(presence.To));
				}

			}

			UpdateItems(presence, ref fromRosterItem, ref toRosterItem);
		}

		private void UpdateItems(Presence presence, ref UserRosterItem fromRosterItem, ref UserRosterItem toRosterItem)
		{
			try
			{
				storages.RosterStorage.SaveRosterItem(presence.From, toRosterItem);
				storages.RosterStorage.SaveRosterItem(presence.To, fromRosterItem);
			}
			catch (JabberException)
			{
				//Load back
				toRosterItem = GetUserRosterItem(presence.From, presence.To);
				fromRosterItem = GetUserRosterItem(presence.To, presence.From);
			}
		}

		private UserRosterItem GetUserRosterItem(Jid user, Jid userInRoster)
		{
			var rosterItem = storages.RosterStorage.GetRosterItem(user, userInRoster);
			if (rosterItem == null)
			{
				rosterItem = new UserRosterItem(userInRoster);
				rosterItem.Subscribtion = SubscriptionType.none;
				rosterItem.Ask = AskType.NONE;
			}
			return rosterItem;
		}

		//
		//"None" = contact and user are not subscribed to each other, and neither has requested a 
		//subscription from the other
		//"None + Pending Out" = contact and user are not subscribed to each other, and user has sent contact 
		//a subscription request but contact has not replied yet
		//"None + Pending In" = contact and user are not subscribed to each other, and contact has sent user 
		//a subscription request but user has not replied yet (note: contact's server SHOULD NOT push or 
		//deliver roster items in this state, but instead SHOULD wait until contact has approved subscription 
		//request from user)
		//"None + Pending Out/In" = contact and user are not subscribed to each other, contact has sent user 
		//a subscription request but user has not replied yet, and user has sent contact a subscription 
		//request but contact has not replied yet
		//"To" = user is subscribed to contact (one-way)
		//"To + Pending In" = user is subscribed to contact, and contact has sent user a subscription request 
		//but user has not replied yet
		//"From" = contact is subscribed to user (one-way)
		//"From + Pending Out" = contact is subscribed to user, and user has sent contact a subscription 
		//request but contact has not replied yet
		//"Both" = user and contact are subscribed to each other (two-way)
		private enum State
		{
			None,
			None_PendingOut,
			None_PendingIn,
			None_PendingInOut,
			To,
			To_PendingIn,
			From,
			From_PendingOut,
			Both
		}

		private State GetState(UserRosterItem itemSendingPresence, UserRosterItem itemRecivingPresence)
		{
			var state = State.None;
			Debug.Assert(itemSendingPresence != null);
			if (itemSendingPresence.Subscribtion == SubscriptionType.none)
			{
				if (itemSendingPresence.Ask == AskType.subscribe)
				{
					state = itemRecivingPresence != null && itemRecivingPresence.Ask == AskType.subscribe ?
						State.None_PendingInOut :
						State.None_PendingIn;
				}
				else
				{
					state = itemRecivingPresence.Ask == AskType.subscribe ?
						State.None_PendingOut :
						State.None;
				}
			}
			else if (itemSendingPresence.Subscribtion == SubscriptionType.to)
			{
				state = itemSendingPresence.Ask == AskType.subscribe ?
					State.To_PendingIn :
					State.To;
			}
			else if (itemSendingPresence.Subscribtion == SubscriptionType.from)
			{
				state = itemRecivingPresence != null && itemRecivingPresence.Ask == AskType.subscribe ?
					State.From_PendingOut :
					State.From;
			}
			else if (itemSendingPresence.Subscribtion == SubscriptionType.both)
			{
				state = State.Both;
			}
			return state;
		}

		private void SetState(State state, UserRosterItem itemSendingPresence, UserRosterItem itemRecivingPresence)
		{
			Debug.Assert(itemSendingPresence != null);
			if (state == State.None)
			{
				itemSendingPresence.Subscribtion = SubscriptionType.none;
				itemSendingPresence.Ask = AskType.NONE;
			}
			else if (state == State.None_PendingIn)
			{
				itemSendingPresence.Subscribtion = SubscriptionType.none;
				itemSendingPresence.Ask = AskType.subscribe;
			}
			else if (state == State.None_PendingInOut)
			{
				itemSendingPresence.Subscribtion = SubscriptionType.none;
				itemSendingPresence.Ask = AskType.subscribe;

				Debug.Assert(itemRecivingPresence != null);

				itemRecivingPresence.Ask = AskType.subscribe;
			}
			else if (state == State.From)
			{
				itemSendingPresence.Subscribtion = SubscriptionType.from;
				itemSendingPresence.Ask = AskType.NONE;
			}
			else if (state == State.From_PendingOut)
			{
				itemSendingPresence.Subscribtion = SubscriptionType.from;
				itemSendingPresence.Ask = AskType.NONE;
				Debug.Assert(itemRecivingPresence != null);

				itemRecivingPresence.Ask = AskType.subscribe;
			}
			else if (state == State.To)
			{
				itemSendingPresence.Subscribtion = SubscriptionType.to;
				itemSendingPresence.Ask = AskType.NONE;
			}
			else if (state == State.To_PendingIn)
			{
				itemSendingPresence.Subscribtion = SubscriptionType.to;
				itemSendingPresence.Ask = AskType.subscribe;
			}
			else if (state == State.Both)
			{
				itemSendingPresence.Subscribtion = SubscriptionType.both;
				itemSendingPresence.Ask = AskType.NONE;
				Debug.Assert(itemRecivingPresence != null);

				itemRecivingPresence.Subscribtion = SubscriptionType.both;
				itemRecivingPresence.Ask = AskType.NONE;
			}
		}

		private void ChangeInboundPresence(State state, UserRosterItem itemSendingPresence, UserRosterItem itemRecivingPresence, PresenceType newType, out bool bRoute, out bool bAutoReply)
		{
			bRoute = false;
			bAutoReply = false;
			//Inbound! change state of reciving item!!!
			if (newType == PresenceType.subscribe)
			{
				switch (state)
				{
					case State.None:
						SetState(State.None_PendingIn, itemSendingPresence, itemRecivingPresence);
						bRoute = true;
						break;
					case State.None_PendingOut:
						SetState(State.None_PendingInOut, itemSendingPresence, itemRecivingPresence);
						bRoute = true;
						break;
					case State.To:
						SetState(State.To_PendingIn, itemSendingPresence, itemRecivingPresence);
						bRoute = true;
						break;
					case State.From:
					case State.From_PendingOut:
					case State.Both:
						bAutoReply = true;
						break;
				}
			}
			if (newType == PresenceType.unsubscribe)
			{
				switch (state)
				{
					case State.None_PendingIn:
						bRoute = true;
						bAutoReply = true;
						SetState(State.None, itemSendingPresence, itemRecivingPresence);
						break;
					case State.None_PendingInOut:
						bRoute = true;
						bAutoReply = true;
						SetState(State.None_PendingOut, itemSendingPresence, itemRecivingPresence);
						break;
					case State.To_PendingIn:
						bRoute = true;
						bAutoReply = true;
						SetState(State.To, itemSendingPresence, itemRecivingPresence);
						break;
					case State.From:
						bRoute = true;
						bAutoReply = true;
						SetState(State.None, itemSendingPresence, itemRecivingPresence);
						break;
					case State.From_PendingOut:
						bRoute = true;
						bAutoReply = true;
						SetState(State.None_PendingOut, itemSendingPresence, itemRecivingPresence);
						break;
					case State.Both:
						bRoute = true;
						bAutoReply = true;
						SetState(State.To, itemSendingPresence, itemRecivingPresence);
						break;
				}
			}
			if (newType == PresenceType.subscribed)
			{
				switch (state)
				{
					case State.None_PendingOut:
						bRoute = true;
						SetState(State.To, itemSendingPresence, itemRecivingPresence);
						break;
					case State.None_PendingInOut:
						bRoute = true;
						SetState(State.To_PendingIn, itemSendingPresence, itemRecivingPresence);
						break;
					case State.From_PendingOut:
						bRoute = true;
						SetState(State.Both, itemSendingPresence, itemRecivingPresence);
						break;
				}

			}
			if (newType == PresenceType.unsubscribed)
			{
				switch (state)
				{
					case State.None_PendingOut:
						bRoute = true;
						SetState(State.None, itemSendingPresence, itemRecivingPresence);
						break;
					case State.None_PendingInOut:
						bRoute = true;
						SetState(State.None_PendingIn, itemSendingPresence, itemRecivingPresence);
						break;
					case State.To:
						bRoute = true;
						SetState(State.None, itemSendingPresence, itemRecivingPresence);
						break;
					case State.To_PendingIn:
						bRoute = true;
						SetState(State.None_PendingIn, itemSendingPresence, itemRecivingPresence);
						break;
					case State.From_PendingOut:
						bRoute = true;
						SetState(State.From, itemSendingPresence, itemRecivingPresence);
						break;
					case State.Both:
						bRoute = true;
						SetState(State.From, itemSendingPresence, itemRecivingPresence);
						break;
				}
			}
		}

		private void ChangeOutboundPresence(State state, UserRosterItem itemSendingPresence, UserRosterItem itemRecivingPresence, PresenceType newType, out bool bRoute)
		{
			//Change state of sending presence!
			bRoute = false;

			if (newType == PresenceType.subscribed)
			{
				switch (state)
				{
					case State.None_PendingIn:
						bRoute = true;
						SetState(State.From, itemSendingPresence, itemRecivingPresence);
						break;
					case State.None_PendingInOut:
						bRoute = true;
						SetState(State.From_PendingOut, itemSendingPresence, itemRecivingPresence);
						break;
					case State.To_PendingIn:
						bRoute = true;
						SetState(State.Both, itemSendingPresence, itemRecivingPresence);
						break;
				}
			}
			if (newType == PresenceType.unsubscribed)
			{
				switch (state)
				{
					case State.None_PendingIn:
						bRoute = true;
						SetState(State.None, itemSendingPresence, itemRecivingPresence);
						break;
					case State.None_PendingInOut:
						bRoute = true;
						SetState(State.None_PendingOut, itemSendingPresence, itemRecivingPresence);
						break;
					case State.To_PendingIn:
						bRoute = true;
						SetState(State.To, itemSendingPresence, itemRecivingPresence);
						break;
					case State.From:
						bRoute = true;
						SetState(State.None, itemSendingPresence, itemRecivingPresence);
						break;
					case State.From_PendingOut:
						bRoute = true;
						SetState(State.None_PendingOut, itemSendingPresence, itemRecivingPresence);
						break;
					case State.Both:
						bRoute = true;
						SetState(State.To, itemSendingPresence, itemRecivingPresence);
						break;
				}
			}
		}

		private void StoreOffline(Presence presence)
		{
			storages.OfflineStorage.SaveOfflinePresence(presence);
		}
	}
}