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
            get { return new Vcard() { Fullname = Name, Description = "© 2008-2015 Assensio System SIA", Url = "http://onlyoffice.com" }; }
        }

        public override void Configure(IDictionary<string, string> properties)
        {
            DiscoInfo.AddIdentity(new DiscoIdentity("server", Name, "im"));
            lock (Handlers)
            {
                Handlers.Add(new ClientNamespaceHandler());
                Handlers.Add(new StartTlsHandler());
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