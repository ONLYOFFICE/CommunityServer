/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Xmpp.Core.protocol.extensions.bosh;
using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Core.utils.Xml.Dom;
using System.Collections.Generic;
using System.Net;
using Uri = ASC.Xmpp.Core.protocol.Uri;

namespace ASC.Xmpp.Server.Gateway
{
    class BoshXmppRequest
    {
        private readonly Body body;
        private readonly HttpListenerContext context;


        public BoshXmppRequest(string id, Body bodyElement, HttpListenerContext context)
        {
            this.context = context;

            body = ElementSerializer.DeSerializeElement<Body>(ElementSerializer.SerializeElement(bodyElement)); // clone

            if (string.IsNullOrEmpty(body.Sid))
            {
                body.Sid = id;
                body.Secure = false;
            }
            body.Ack = body.Rid;
            body.RemoveAttribute("rid");
            body.To = null;
            if (body.HasAttribute("xmpp:version") || body.HasAttribute("xmpp:restart"))
            {
                body.SetAttribute("xmlns:xmpp", "urn:xmpp:xbosh");
            }
            body.RemoveAllChildNodes();
        }

        public void SendAndClose(IEnumerable<Node> buffer, bool terminate)
        {
            foreach (var node in buffer)
            {
                body.AddChild(node);
                if (node.Namespace == Uri.STREAM)
                {
                    body.SetAttribute("xmlns:stream", Uri.STREAM);
                }
            }

            if (terminate)
            {
                body.Type = BoshType.terminate;
            }
            BoshXmppHelper.SendAndCloseResponse(context, body);
        }

        public void Close(bool terminate)
        {
            if (terminate)
            {
                BoshXmppHelper.TerminateBoshSession(context, body);
            }
            else
            {
                BoshXmppHelper.SendAndCloseResponse(context, new Body());
            }
        }
    }
}
