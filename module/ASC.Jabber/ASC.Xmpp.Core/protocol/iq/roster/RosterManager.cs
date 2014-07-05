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

/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2008 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */ 

using System;
using System.Collections;

using ASC.Xmpp.protocol;
using ASC.Xmpp.protocol.iq;
using ASC.Xmpp.protocol.client;

namespace ASC.Xmpp.protocol.iq.roster
{
    using client;

    /// <summary>
	/// Helper Class that makes it easier to manage your roster
	/// </summary>
	public class RosterManager
	{
		private XmppClientConnection	m_connection	= null;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="con">The XmppClientConnection on which the RosterManager should send the packets</param>
		public RosterManager(XmppClientConnection con)
		{		
			m_connection = con;
		}

		/// <summary>
		/// Removes a Rosteritem from the Roster
		/// </summary>
		/// <param name="jid">The BARE jid of the rosteritem that should be removed</param>
		public void RemoveRosterItem(Jid jid)
		{
			RosterIq riq = new RosterIq();
			riq.Type = IqType.set;
				
			RosterItem ri = new RosterItem();
			ri.Jid = jid;
			ri.Subscription = SubscriptionType.remove;

			riq.Query.AddRosterItem(ri);
				
			m_connection.Send(riq);
		}

		/// <summary>
		/// Add a Rosteritem to the Roster
		/// </summary>
		/// <param name="jid">The BARE jid of the rosteritem that should be removed</param>
		public void AddRosterItem(Jid jid)
		{
			AddRosterItem(jid, null, new string[] {});
		}

		/// <summary>
		/// Update a Rosteritem
		/// </summary>
		/// <param name="jid"></param>
		public void UpdateRosterItem(Jid jid)
		{
			AddRosterItem(jid, null, new string[] {});
		}

		/// <summary>
		/// Add a Rosteritem to the Roster
		/// </summary>
		/// <param name="jid">The BARE jid of the rosteritem that should be removed</param>
		/// <param name="nickname">Nickname for the RosterItem</param>
		public void AddRosterItem(Jid jid, string nickname)
		{
			AddRosterItem(jid, nickname, new string[] {});
		}

		/// <summary>
		/// Update a Rosteritem
		/// </summary>
		/// <param name="jid"></param>
		/// <param name="nickname"></param>
		public void UpdateRosterItem(Jid jid, string nickname)
		{
			AddRosterItem(jid, nickname, new string[] {});
		}

		/// <summary>
		/// Add a Rosteritem to the Roster
		/// </summary>
		/// <param name="jid">The BARE jid of the rosteritem that should be removed</param>
		/// <param name="nickname">Nickname for the RosterItem</param>
		/// <param name="group">The group to which the roteritem should be added</param>
		public void AddRosterItem(Jid jid, string nickname, string group)
		{
			AddRosterItem(jid, nickname, new string[] {group});
		}

		/// <summary>
		/// Update a Rosteritem
		/// </summary>
		/// <param name="jid"></param>
		/// <param name="nickname"></param>
		/// <param name="group"></param>
		public void UpdateRosterItem(Jid jid, string nickname, string group)
		{
			AddRosterItem(jid, nickname, new string[] {group});
		}

		/// <summary>
		/// Add a Rosteritem to the Roster
		/// </summary>
		/// <param name="jid">The BARE jid of the rosteritem that should be removed</param>
		/// <param name="nickname">Nickname for the RosterItem</param>
		/// <param name="group">An Array of groups when you want to add the Rosteritem to multiple groups</param>
		public void AddRosterItem(Jid jid, string nickname, string[] group)
		{
			RosterIq riq = new RosterIq();
			riq.Type = IqType.set;
				
			RosterItem ri = new RosterItem();
			ri.Jid	= jid;
			
			if (nickname != null)
				ri.Name	= nickname;
			
			foreach (string g in group)
			{
				ri.AddGroup(g);			
			}

			riq.Query.AddRosterItem(ri);
				
			m_connection.Send(riq);
		}

		/// <summary>
		/// Update a Rosteritem
		/// </summary>
		/// <param name="jid"></param>
		/// <param name="nickname"></param>
		/// <param name="group"></param>
		public void UpdateRosterItem(Jid jid, string nickname, string[] group)
		{
			AddRosterItem(jid, nickname, group);
		}
		
	}
}
