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
