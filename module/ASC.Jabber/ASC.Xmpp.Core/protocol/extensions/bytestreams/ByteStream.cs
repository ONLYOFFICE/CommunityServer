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

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.bytestreams
{
    /*
        <iq type='set' 
           from='initiator@host1/foo' 
           to='proxy.host3' 
           id='activate'>
           <query xmlns='http://jabber.org/protocol/bytestreams' sid='mySID'>
               <activate>target@host2/bar</activate>
           </query>
        </iq>
     
      
        <xs:element name='query'>
           <xs:complexType>
             <xs:choice>
               <xs:element ref='streamhost' minOccurs='0' maxOccurs='unbounded'/>
               <xs:element ref='streamhost-used' minOccurs='0'/>
               <xs:element name='activate' type='empty' minOccurs='0'/>
             </xs:choice>
             <xs:attribute name='sid' type='xs:string' use='optional'/>
             <xs:attribute name='mode' use='optional' default='tcp'>
               <xs:simpleType>
                 <xs:restriction base='xs:NCName'>
                   <xs:enumeration value='tcp'/>
                   <xs:enumeration value='udp'/>
                 </xs:restriction>
               </xs:simpleType>
             </xs:attribute>
           </xs:complexType>
        </xs:element>
    */

    /// <summary>
    ///   ByteStreams
    /// </summary>
    public class ByteStream : Element
    {
        public ByteStream()
        {
            TagName = "query";
            Namespace = Uri.BYTESTREAMS;
        }

        public string Sid
        {
            set { SetAttribute("sid", value); }
            get { return GetAttribute("sid"); }
        }

        public Mode Mode
        {
            get { return (Mode) GetAttributeEnum("mode", typeof (Mode)); }
            set
            {
                if (value != Mode.NONE)
                    SetAttribute("mode", value.ToString());
                else
                    RemoveAttribute("mode");
            }
        }

        /// <summary>
        ///   The activate Element
        /// </summary>
        public Activate Activate
        {
            get { return SelectSingleElement(typeof (Activate)) as Activate; }
            set
            {
                if (HasTag(typeof (Activate)))
                    RemoveTag(typeof (Activate));

                if (value != null)
                    AddChild(value);
            }
        }

        public StreamHostUsed StreamHostUsed
        {
            get { return SelectSingleElement(typeof (StreamHostUsed)) as StreamHostUsed; }
            set
            {
                if (HasTag(typeof (StreamHostUsed)))
                    RemoveTag(typeof (StreamHostUsed));

                if (value != null)
                    AddChild(value);
            }
        }

        /// <summary>
        ///   Add a StreamHost
        /// </summary>
        /// <returns> </returns>
        public StreamHost AddStreamHost()
        {
            var sh = new StreamHost();
            AddChild(sh);
            return sh;
        }

        /// <summary>
        ///   Add a StreamHost
        /// </summary>
        /// <param name="sh"> </param>
        /// <returns> </returns>
        public StreamHost AddStreamHost(StreamHost sh)
        {
            AddChild(sh);
            return sh;
        }

        /// <summary>
        ///   Add a StreamHost
        /// </summary>
        /// <param name="jid"> </param>
        /// <param name="host"> </param>
        /// <returns> </returns>
        public StreamHost AddStreamHost(Jid jid, string host)
        {
            var sh = new StreamHost(jid, host);
            AddChild(sh);
            return sh;
        }

        /// <summary>
        ///   Add a StreamHost
        /// </summary>
        /// <param name="jid"> </param>
        /// <param name="host"> </param>
        /// <param name="port"> </param>
        /// <returns> </returns>
        public StreamHost AddStreamHost(Jid jid, string host, int port)
        {
            var sh = new StreamHost(jid, host, port);
            AddChild(sh);
            return sh;
        }

        /// <summary>
        ///   Add a StreamHost
        /// </summary>
        /// <param name="jid"> </param>
        /// <param name="host"> </param>
        /// <param name="port"> </param>
        /// <param name="zeroconf"> </param>
        /// <returns> </returns>
        public StreamHost AddStreamHost(Jid jid, string host, int port, string zeroconf)
        {
            var sh = new StreamHost(jid, host, port, zeroconf);
            AddChild(sh);
            return sh;
        }

        /// <summary>
        ///   Get the list of streamhosts
        /// </summary>
        /// <returns> </returns>
        public StreamHost[] GetStreamHosts()
        {
            ElementList nl = SelectElements(typeof (StreamHost));
            var hosts = new StreamHost[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                hosts[i] = (StreamHost) e;
                i++;
            }
            return hosts;
        }
    }
}