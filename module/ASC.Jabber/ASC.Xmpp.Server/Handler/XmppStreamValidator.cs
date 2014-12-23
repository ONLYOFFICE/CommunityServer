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
using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.auth;
using ASC.Xmpp.Core.protocol.iq.register;
using ASC.Xmpp.Core.utils.Idn;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Streams;
using Error = ASC.Xmpp.Core.protocol.Error;
using Uri = ASC.Xmpp.Core.protocol.Uri;

namespace ASC.Xmpp.Server.Handler
{
    class XmppStreamValidator
    {
        public bool ValidateStanza(Stanza stanza, XmppStream stream, XmppHandlerContext context)
        {
            Element result = null;
            if (stream.Namespace == Uri.CLIENT) result = ValidateClientStanza(stanza, stream);
            if (stream.Namespace == Uri.SERVER) result = ValidateServerStanza(stanza, stream);

            if (result == null) return true;

            if (result is Stanza)
            {
                context.Sender.SendTo(stream, result);
            }
            else if (result is Error)
            {
                context.Sender.SendToAndClose(stream, result);
            }
            else
            {
                return true;
            }
            return false;
        }

        private Element ValidateClientStanza(Stanza stanza, XmppStream stream)
        {
            if (!stream.Authenticated)
            {
                if (!(stanza is AuthIq) && (stanza is IQ && !(((IQ)stanza).Query is Register))) return XmppStanzaError.ToNotAuthorized(stanza);
            }

            //remove empty jids
            if (stanza.HasFrom && string.IsNullOrEmpty(stanza.From.ToString())) stanza.From = null;
            if (stanza.HasTo && string.IsNullOrEmpty(stanza.To.ToString())) stanza.To = null;

            //prep strings
            stanza.From = NodePrep(stanza.From);
            stanza.To = NodePrep(stanza.To);

            if (!ValidateJid(stanza.From) || !ValidateJid(stanza.To)) return XmppStanzaError.ToBadRequest(stanza);

            if (stanza.HasFrom)
            {
                if (!stream.JidBinded(stanza.From))
                {
                    // return null if we have from in bind iq (for qutIM 0.3 client)
                    if (!(stanza is IQ) || ((IQ)stanza).Bind == null || ((IQ)stanza).Bind.Resource != stanza.From.Resource)
                    {
                        return XmppStreamError.InvalidFrom;
                    }
                }
            }
            else
            {
                if (stream.MultipleResources) return XmppStanzaError.ToConflict(stanza);
                stanza.From = new Jid(string.Format("{0}@{1}/{2}", stream.User, stream.Domain, 0 < stream.Resources.Count ? stream.Resources[0] : null));
            }

            if (stanza is Message)
            {
                var message = (Message)stanza;
                if (message.Type == MessageType.chat && message.To == null) return XmppStanzaError.ToRecipientUnavailable(stanza);
            }
            return null;
        }

        private bool ValidateJid(Jid jid)
        {
            if (jid == null) return true;
            if (jid.HasUser)
            {
                if (jid.User != Jid.EscapeNode(jid.User)) return false;
                if (jid.User != Stringprep.NodePrep(jid.User)) return false;
            }
            return true;
        }

        private Jid NodePrep(Jid jid)
        {
            if (jid != null)
            {
                if (jid.HasUser)
                {
                    jid.User = Stringprep.NodePrep(jid.User);
                }
                if (jid.HasResource)
                {
                    jid.Resource = Stringprep.ResourcePrep(jid.Resource);
                }
                //BUG: Many faults here in log
                jid.Server = Stringprep.NamePrep(jid.Server);
            }
            return jid;
        }

        private Element ValidateServerStanza(Stanza stanza, XmppStream stream)
        {
            if (!stanza.HasTo || !stanza.HasFrom) return XmppStreamError.ImproperAddressing;
            return null;
        }
    }
}
