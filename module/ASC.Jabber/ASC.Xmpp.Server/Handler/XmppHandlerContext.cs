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


using System;
using ASC.Xmpp.Server.Authorization;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Users;

namespace ASC.Xmpp.Server.Handler
{
    public class XmppHandlerContext
    {
        public IServiceProvider ServiceProvider
        {
            get;
            private set;
        }

        public IXmppSender Sender
        {
            get { return (IXmppSender)ServiceProvider.GetService(typeof(IXmppSender)); }
        }

        public UserManager UserManager
        {
            get { return (UserManager)ServiceProvider.GetService(typeof(UserManager)); }
        }

        public XmppSessionManager SessionManager
        {
            get { return (XmppSessionManager)ServiceProvider.GetService(typeof(XmppSessionManager)); }
        }

        public StorageManager StorageManager
        {
            get { return (StorageManager)ServiceProvider.GetService(typeof(StorageManager)); }
        }

        public XmppGateway XmppGateway
        {
            get { return (XmppGateway)ServiceProvider.GetService(typeof(IXmppReceiver)); }
        }

        public AuthManager AuthManager
        {
            get { return (AuthManager)ServiceProvider.GetService(typeof(AuthManager)); }
        }

        public XmppHandlerContext(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            ServiceProvider = serviceProvider;
        }
    }
}
