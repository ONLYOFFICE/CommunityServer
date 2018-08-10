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


using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.bytestreams
{
    /*
        <iq type='result' 
            from='target@host2/bar' 
            to='initiator@host1/foo' 
            id='initiate'>
          <query xmlns='http://jabber.org/protocol/bytestreams'>
            <streamhost-used jid='proxy.host3'/>
          </query>
        </iq>
    */

    /// <summary>
    ///   The <streamhost-used /> element indicates the StreamHost connected to. This element has a single attribute for the JID of the StreamHost to which the Target connected. This element MUST NOT contain any content node. The "jid" attribute specifies the full JID of the StreamHost. This attribute MUST be present, and MUST be a valid JID for use with an &lt;iq/&gt;.
    /// </summary>
    public class StreamHostUsed : Element
    {
        public StreamHostUsed()
        {
            TagName = "streamhost-used";
            Namespace = Uri.BYTESTREAMS;
        }

        public StreamHostUsed(Jid jid) : this()
        {
            Jid = jid;
        }

        /// <summary>
        ///   Jid of the streamhost
        /// </summary>
        public Jid Jid
        {
            get
            {
                if (HasAttribute("jid"))
                    return new Jid(GetAttribute("jid"));
                else
                    return null;
            }
            set
            {
                if (value != null)
                    SetAttribute("jid", value.ToString());
                else
                    RemoveAttribute("jid");
            }
        }
    }
}