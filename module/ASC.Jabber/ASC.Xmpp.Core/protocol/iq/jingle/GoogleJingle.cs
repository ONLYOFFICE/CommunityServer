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

namespace ASC.Xmpp.Core.protocol.iq.jingle
{
    public class Stun : Element
    {
        public Stun()
        {
            TagName = "stun";
            Namespace = Uri.IQ_GOOGLE_JINGLE;
        }

        public Server[] GetServers()
        {
            ElementList nl = SelectElements(typeof (Server));
            int i = 0;
            var result = new Server[nl.Count];
            foreach (Server ri in nl)
            {
                result[i] = ri;
                i++;
            }
            return result;
        }

        public void AddServer(Server r)
        {
            ChildNodes.Add(r);
        }
    }

    public class Server : Element
    {
        public Server()
        {
            TagName = "server";
            Namespace = Uri.IQ_GOOGLE_JINGLE;
        }

        public Server(string host, int udp) : this()
        {
            Host = host;
            Udp = udp;
        }

        public string Host
        {
            get { return GetAttribute("host"); }

            set { SetAttribute("host", value); }
        }

        public int Udp
        {
            get { return GetAttributeInt("udp"); }

            set { SetAttribute("udp", value); }
        }
    }


    public class GoogleJingle : Element
    {
        public GoogleJingle()
        {
            TagName = "query";
            Namespace = Uri.IQ_GOOGLE_JINGLE;
        }


        public virtual Stun Stun
        {
            get { return SelectSingleElement(typeof (Stun)) as Stun; }

            set
            {
                RemoveTag(typeof (Stun));
                if (value != null)
                {
                    AddChild(value);
                }
            }
        }
    }
}