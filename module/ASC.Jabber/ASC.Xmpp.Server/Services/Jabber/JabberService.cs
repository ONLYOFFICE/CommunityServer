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
using ASC.Xmpp.Core.protocol.iq.disco;
using ASC.Xmpp.Core.protocol.iq.vcard;
using ASC.Xmpp.Server.Authorization;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;
using System;
using System.Collections.Generic;

namespace ASC.Xmpp.Server.Services.Jabber
{
    class JabberService : XmppServiceBase
    {
        private MessageAnnounceHandler messageAnnounceHandler;

        public override Vcard Vcard
        {
            get { return new Vcard() { Fullname = Name, Description = "© 2008-2014 Assensio System SIA", Url = "http://onlyoffice.com" }; }
        }

        public override void Configure(IDictionary<string, string> properties)
        {
            DiscoInfo.AddIdentity(new DiscoIdentity("server", Name, "im"));
            lock (Handlers)
            {
                Handlers.Add(new ClientNamespaceHandler());
                Handlers.Add(new AuthHandler());
                Handlers.Add(new AuthTMTokenHandler());
                Handlers.Add(new BindSessionHandler());
                Handlers.Add(new RosterHandler());
                Handlers.Add(new VCardHandler());
                Handlers.Add(new VerTimePingHandler());
                Handlers.Add(new PrivateHandler());
                Handlers.Add(new PresenceHandler());
                Handlers.Add(new MessageHandler());
                Handlers.Add(new MessageArchiveHandler());
                Handlers.Add(new LastHandler());
                Handlers.Add(new RegisterHandler());
                Handlers.Add(new TransferHandler());
                Handlers.Add(new CommandHandler());
                Handlers.Add(new OfflineProvider(Jid));
                Handlers.Add(new DiscoHandler(Jid));
            }
            messageAnnounceHandler = new MessageAnnounceHandler();
        }

        protected override void OnRegisterCore(XmppHandlerManager handlerManager, XmppServiceManager serviceManager, IServiceProvider serviceProvider)
        {
            var jid = new Jid(Jid.ToString());
            jid.Resource = MessageAnnounceHandler.ANNOUNCE;
            handlerManager.AddXmppHandler(jid, messageAnnounceHandler);
        }

        protected override void OnUnregisterCore(XmppHandlerManager handlerManager, XmppServiceManager serviceManager, IServiceProvider serviceProvider)
        {
            handlerManager.RemoveXmppHandler(messageAnnounceHandler);
        }
    }
}