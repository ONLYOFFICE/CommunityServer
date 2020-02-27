/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
        <streamhost 
            jid='proxy.host3' 
            host='24.24.24.1' 
            zeroconf='_jabber.bytestreams'/>
        <xs:element name='streamhost'>
            <xs:complexType>
              <xs:simpleContent>
                <xs:extension base='empty'>
                  <xs:attribute name='jid' type='xs:string' use='required'/>
                  <xs:attribute name='host' type='xs:string' use='required'/>
                  <xs:attribute name='zeroconf' type='xs:string' use='optional'/>
                  <xs:attribute name='port' type='xs:string' use='optional'/>
                </xs:extension>
              </xs:simpleContent>
            </xs:complexType>
        </xs:element>
    */

    public class StreamHost : Element
    {
        public StreamHost()
        {
            TagName = "streamhost";
            Namespace = Uri.BYTESTREAMS;
        }

        public StreamHost(Jid jid, string host) : this()
        {
            Jid = jid;
            Host = host;
        }

        public StreamHost(Jid jid, string host, int port) : this(jid, host)
        {
            Port = port;
        }

        public StreamHost(Jid jid, string host, int port, string zeroconf) : this(jid, host, port)
        {
            Zeroconf = zeroconf;
        }

        /// <summary>
        ///   a port associated with the hostname or IP address for SOCKS5 communications over TCP
        /// </summary>
        public int Port
        {
            get { return GetAttributeInt("port"); }
            set { SetAttribute("port", value); }
        }

        /// <summary>
        ///   the hostname or IP address of the StreamHost for SOCKS5 communications over TCP
        /// </summary>
        public string Host
        {
            get { return GetAttribute("host"); }
            set { SetAttribute("host", value); }
        }

        /// <summary>
        ///   The XMPP/Jabber id of the streamhost
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

        /// <summary>
        ///   a zeroconf [5] identifier to which an entity may connect, for which the service identifier and protocol name SHOULD be "_jabber.bytestreams".
        /// </summary>
        public string Zeroconf
        {
            get { return GetAttribute("zeroconf"); }
            set { SetAttribute("zeroconf", value); }
        }
    }
}