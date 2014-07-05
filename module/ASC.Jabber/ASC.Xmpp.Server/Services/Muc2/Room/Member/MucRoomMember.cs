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

using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.disco;
using ASC.Xmpp.Core.protocol.iq.vcard;
using ASC.Xmpp.Core.protocol.x.muc;
using Error = ASC.Xmpp.Core.protocol.client.Error;
using Item = ASC.Xmpp.Core.protocol.x.muc.Item;

namespace ASC.Xmpp.Server.Services.Muc2.Room.Member
{
	using System;
	using Gateway;
	using Handler;
	using Streams;
	using Item = Item;

	internal class MucRoomMember : XmppServiceBase, IEquatable<MucRoomMember>
	{
		private readonly MucRoom room;
		private XmppStream stream;
		private readonly XmppHandlerContext context;
		private readonly IXmppSender sender;

		internal event MemberAddressChangeDelegate AddressChanged = null;

		private void InvokeAddressChanged(Jid newAddress)
		{
			MemberAddressChangeDelegate evt = AddressChanged;
			if (evt != null)
			{
				evt(this, newAddress);
			}
		}

		internal event MemberActionDelegate PresenceChanged = null;

        private void SoftInvokePresenceChanged(MucRoomMember member)
        {
            try
            {
                InvokePresenceChanged(member);
            }
            catch (Exception)
            {
            }
        }

		private void InvokePresenceChanged(MucRoomMember member)
		{
			MemberActionDelegate evt = PresenceChanged;
			if (evt != null)
			{
				evt(member);
			}
		}

		internal event MemberActionBroadcastDelegate PresenceBroadcasted = null;

		private void InvokePresenceBroadcasted(Presence presence)
		{
			MemberActionBroadcastDelegate broadcasted = PresenceBroadcasted;
			if (broadcasted != null)
			{
				broadcasted(this, presence);
			}
		}

		internal event MemberActionDelegate Unavailible = null;

		private void InvokeUnavailible(MucRoomMember member)
		{
			MemberActionDelegate unavailible = Unavailible;
			if (unavailible != null)
			{
				unavailible(member);
			}
		}

		internal event MemberActionDelegate AffilationChanged = null;

		private void InvokeAffilationChanged(MucRoomMember member)
		{
			MemberActionDelegate evt = AffilationChanged;
			if (evt != null)
			{
				evt(member);
			}
		}

		internal event MemberActionDelegate RoleChanged = null;

		private void InvokeRoleChanged(MucRoomMember member)
		{
			MemberActionDelegate evt = RoleChanged;
			if (evt != null)
			{
				evt(member);
			}
		}

		private Affiliation affiliation;
		internal Affiliation Affiliation
		{
			get { return affiliation; }
			set
			{
				if (affiliation != value)
				{
					affiliation = value;
					InvokeAffilationChanged(this);
				}
			}
		}

		private Role role;
		internal Role Role
		{
			get { return role; }
			set
			{
				if (role != value)
				{
					role = value;
					InvokeRoleChanged(this);
				}
			}
		}

		internal string Nick
		{
			get
			{
				return Jid.Resource;
			}
			set
			{
				if (value != Nick)
				{
					Presence unavailPres = GetPresence(PresenceType.unavailable);
					User user = User;
					user.Item.Nickname = value;
					user.Status = new Status(303);
					unavailPres.AddChild(user);
					InvokePresenceBroadcasted(unavailPres);


					//Change nick
					InvokeAddressChanged(new Jid(Jid.User, Jid.Server, value));

					// Broadcast new nick
					Presence newPresence = GetPresence(PresenceType.available);
					user = User;
					newPresence.AddChild(user);
					InvokePresenceBroadcasted(newPresence);
				}
			}
		}

		internal string UserName
		{
			get
			{
				return RealJid.Resource;
			}
		}

		private PresenceType presenceType;
		internal PresenceType PresenceType
		{
			get { return presenceType; }
			set
			{
				if (presenceType != value)
				{
					presenceType = value;
                    SoftInvokePresenceChanged(this);
					if (presenceType == PresenceType.unavailable)
					{
						InvokeUnavailible(this);
					}
				}
			}
		}

		internal Presence GetPresence(PresenceType type)
		{
			Presence presence = new Presence();
			presence.Type = type;
			presence.From = Jid;
			return presence;
		}

		private Presence setPresence = null;

		internal Presence Presence
		{
			get
			{
				Presence presence = new Presence();
				presence.Type = PresenceType;
				presence.From = Jid;
				if (setPresence != null)
				{
					presence.Show = setPresence.Show;
					presence.Status = setPresence.Status;
				}
				presence.AddChild(User);
				return presence;
			}
		}

		internal User User
		{
			get
			{
				User user = new User();
				Item item = new Item();
				user.Item = item;
				user.Item.Affiliation = Affiliation;
				user.Item.Role = Role;
				user.Item.Jid = RealJid;
			    user.Item.Actor = Actor;
			    user.Item.Reason = Reason;
			    user.Status = Status;
				return user;
			}
		}

        public Actor Actor { get; set; }
        public string Reason { get; set; }
        public Status Status { get; set; }

        public void Kick(Actor initiator, string reason)
        {
            //Remove me from room
            Reason = reason;
            Actor = initiator;
            Status = new Status(307);
            Role = Role.none;
        }

		public MucRoomMember(MucRoom room, Jid memberJid, Jid realJid, XmppStream stream, XmppHandlerContext context)
		{
			if (room == null)
			{
				throw new ArgumentNullException("room");
			}
			if (memberJid == null)
			{
				throw new ArgumentNullException("memberJid");
			}
			if (realJid == null)
			{
				throw new ArgumentNullException("realJid");
			}
			if (context == null)
			{
				throw new ArgumentNullException("sender");
			}
			this.room = room;
			this.stream = stream;
			this.context = context;
			this.sender = context.Sender;

			//Register disconect
			context.SessionManager.SessionUnavailable += SessionManager_SessionUnavailable;

			ParentService = room;
			RealJid = realJid;
			Jid = memberJid;
			Name = memberJid.Resource;
			presenceType = PresenceType.unavailable;
			//Create handler
			Handlers.Add(new MucRoomMemberStanzaHandler(this));
			Handlers.Add(new MucRoomMemberDiscoHandler(Jid, RealJid));

			DiscoInfo.AddIdentity(new DiscoIdentity("text", Name, "member"));
			DiscoInfo.AddFeature(new DiscoFeature(Core.protocol.Uri.DISCO_INFO));
            DiscoInfo.AddFeature(new DiscoFeature(Core.protocol.Uri.DISCO_ITEMS));
		}

		void SessionManager_SessionUnavailable(object sender, ASC.Xmpp.Server.Session.XmppSessionArgs e)
		{
			if (ReferenceEquals(e.Session.Stream, stream))
			{
				//Session shutdown
				if (PresenceType != PresenceType.unavailable)
				{
					PresenceType = PresenceType.unavailable;
				}
			}
		}

        protected override void OnUnregisterCore(XmppHandlerManager handlerManager, XmppServiceManager serviceManager, IServiceProvider serviceProvider)
        {
            base.OnUnregisterCore(handlerManager, serviceManager, serviceProvider);
            context.SessionManager.SessionUnavailable -= SessionManager_SessionUnavailable;
        }

		public Jid RealJid { get; set; }

		public XmppStream Stream
		{
			get { return stream; }
			set { stream = value; }
		}

		public Jid RoomFrom
		{
			get { return room.Jid; }
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
		public bool Equals(MucRoomMember other)
		{
			if (other == null)
			{
				return false;
			}
			if (other.Jid != null)
			{
				return other.Jid.Equals(Jid);
			}
			return ReferenceEquals(other.Jid, Jid);
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
			return 0;
		}

		public static bool operator ==(MucRoomMember left, MucRoomMember right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(MucRoomMember left, MucRoomMember right)
		{
			return !Equals(left, right);
		}

		public void Send(Stanza stanza)
		{
			stanza.To = RealJid;
			sender.SendTo(Stream, stanza);
		}

        public void EnterRoom()
        {
            //Enter with default params
            EnterRoom(room.RoomSettings.GetEnterStatusCodes());
        }

	    public void EnterRoom(params int[] codes)
		{
			PresenceType = PresenceType.available;

			//Create presence and send it back
			Presence backPresence = Presence;
			backPresence.RemoveAllChildNodes();
			User user = User;
			foreach (int code in codes)
			{
				user.AddChild(new Status(code));
			}
			user.AddChild(new Status(110));//Entered
			backPresence.AddChild(user);
			Send(backPresence);


		}

		public Presence Conflict()
		{
			Presence presence = new Presence() { Type = PresenceType.error };
			presence.AddChild(new Muc());
			presence.Error = new Error(ErrorCondition.Conflict);
			presence.From = room.Jid;
			return presence;
		}

		public Jid ResolveRoomJid(Jid @from)
		{
			MucRoomMember member = room.GetRealMember(from);
			if (member != null)
			{
				return member.Jid;
			}
			return null;
		}

		public void ChangePesence(Presence presence)
		{
			setPresence = presence;
			PresenceType = presence.Type;
		}

		public Vcard GetVcard()
		{
			return room.GetMemberVcard(this);
		}

	    public void ReEnterRoom()
	    {
            room.MemberReenter(this);
	    }
	}
}