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

// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="MucService.cs">
//   
// </copyright>
// <summary>
//   (c) Copyright Ascensio System Limited 2008-2009
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.iq.disco;

namespace ASC.Xmpp.Server.Services.Muc2
{
	using System;
	using System.Collections.Generic;
	using ASC.Xmpp.Server.Services.Jabber;
	using ASC.Xmpp.Server.Services.Muc2.Room.Settings;
	using Handler;
	using Room;
	using Storage;
	using Storage.Interface;
    using Uri = Uri;

	internal class MucService : XmppServiceBase
	{
		private XmppHandlerManager handlerManager;

		#region Properties

		public IMucStore MucStorage
		{
			get
			{
				return ((StorageManager)context.GetService(typeof(StorageManager))).MucStorage;
			}
		}

		public IVCardStore VcardStorage
		{
			get
			{
				return ((StorageManager)context.GetService(typeof(StorageManager))).VCardStorage;
			}
		}

		public XmppServiceManager ServiceManager
		{
			get { return ((XmppServiceManager)context.GetService(typeof(XmppServiceManager))); }
		}

		public XmppHandlerManager HandlerManager
		{
			get { return handlerManager; }
		}

		#endregion

		public override void Configure(IDictionary<string, string> properties)
		{
			base.Configure(properties);
	
			DiscoInfo.AddIdentity(new DiscoIdentity("text", Name, "conference"));
			DiscoInfo.AddFeature(new DiscoFeature(Uri.MUC));
			DiscoInfo.AddFeature(new DiscoFeature(Features.FEAT_MUC_ROOMS));

			Handlers.Add(new MucStanzaHandler(this));
			Handlers.Add(new VCardHandler());
			Handlers.Add(new ServiceDiscoHandler(Jid));
		}

		protected override void OnRegisterCore(XmppHandlerManager handlerManager, XmppServiceManager serviceManager, IServiceProvider provider)
		{
			context = provider;
			this.handlerManager = handlerManager;
			LoadRooms();
		}

		private void LoadRooms()
		{
			List<MucRoomInfo> rooms = MucStorage.GetMucs(Jid.Server);
			foreach (MucRoomInfo room in rooms)
			{
				CreateRoom(room.Jid, room.Description);
			}
		}


		private IServiceProvider context;

		/// <summary>
		/// </summary>
		/// <param name="name">
		/// </param>
		/// <param name="description">
		/// </param>
		/// <returns>
		/// </returns>
		internal MucRoom CreateRoom(Jid roomJid, string description)
		{
            MucRoom room = new MucRoom(roomJid, roomJid.User, this, context);
			room.ParentService = this;
			ServiceManager.RegisterService(room);
			return room;
		}

		public void RemoveRoom(MucRoom room)
		{
			ServiceManager.UnregisterService(room.Jid);
            MucStorage.RemoveMuc(room.Jid);
		}
	}
}