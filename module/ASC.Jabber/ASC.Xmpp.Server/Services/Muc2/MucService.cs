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


// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="MucService.cs">
//   
// </copyright>
// <summary>
//   (c) Copyright Ascensio System Limited 2008-2009
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.iq.disco;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Services.Jabber;
using ASC.Xmpp.Server.Services.Muc2.Room;
using ASC.Xmpp.Server.Services.Muc2.Room.Settings;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Storage.Interface;
using System;
using System.Collections.Generic;

namespace ASC.Xmpp.Server.Services.Muc2
{
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
            DiscoInfo.AddFeature(new DiscoFeature(ASC.Xmpp.Core.protocol.Uri.MUC));
            DiscoInfo.AddFeature(new DiscoFeature(Features.FEAT_MUC_ROOMS));
            lock (Handlers)
            {
                Handlers.Add(new MucStanzaHandler(this));
                Handlers.Add(new VCardHandler());
                Handlers.Add(new ServiceDiscoHandler(Jid));
            }
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