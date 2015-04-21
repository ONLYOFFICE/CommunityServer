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
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.roster;
using ASC.Xmpp.Core.utils.Xml.Dom;
using System;
using System.Collections.Generic;
using RosterItem = ASC.Xmpp.Core.protocol.iq.roster.RosterItem;

namespace ASC.Xmpp.Server.Storage
{
	public class UserRosterItem
	{
		public Jid Jid
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			set;
		}

		public SubscriptionType Subscribtion
		{
			get;
			set;
		}

		public AskType Ask
		{
			get;
			set;
		}

		public List<string> Groups
		{
			get;
			private set;
		}

		public UserRosterItem(Jid jid)
		{
			if (jid == null) throw new ArgumentNullException("jid");

			Jid = new Jid(jid.Bare.ToLowerInvariant());
			Groups = new List<string>();
		}

		public RosterItem ToRosterItem()
		{
			var ri = new RosterItem(Jid, Name)
			{
				Subscription = Subscribtion,
				Ask = Ask,
			};
            Groups.ForEach(g => ri.AddGroup(g));
			return ri;
		}

		public static UserRosterItem FromRosterItem(RosterItem ri)
		{
			var item = new UserRosterItem(ri.Jid)
			{
				Name = ri.Name,
				Ask = ri.Ask,
				Subscribtion = ri.Subscription,
			};

            foreach (Element element in ri.GetGroups())
            {
                item.Groups.Add(element.Value);
            }

			return item;
		}

		public IQ GetRosterIq(Jid to)
		{
			var iq = new IQ(IqType.set);
			var roster = new Roster();
			roster.AddRosterItem(ToRosterItem());
			iq.Query = roster;
			iq.To = to.BareJid;
			return iq;
		}

		public override string ToString()
		{
			return string.IsNullOrEmpty(Name) ? Jid.ToString() : Name;
		}

		public override bool Equals(object obj)
		{
			var i = obj as UserRosterItem;
			return i != null && i.Jid == Jid;
		}

		public override int GetHashCode()
		{
			return Jid.GetHashCode();
		}
	}
}