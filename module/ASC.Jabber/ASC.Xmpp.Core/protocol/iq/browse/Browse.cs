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

// JEP-0011: Jabber Browsing
//
// This JEP defines a way to describe information about Jabber entities and the relationships between entities. 
// Note: This JEP is superseded by JEP-0030: Service Discovery.

// WARNING: This JEP has been deprecated by the Jabber Software Foundation. 
// Implementation of the protocol described herein is not recommended. Developers desiring similar functionality should 
// implement the protocol that supersedes this one (if any).

// Most components and gateways still dont implement Service discovery. So we must use jabber:iq:browse for them until everything
// is replaced with JEP 30 (Service Discovery).

namespace ASC.Xmpp.Core.protocol.iq.browse
{
    /// <summary>
    ///   Summary description for Browse.
    /// </summary>
    public class Browse : Element
    {
        public Browse()
        {
            TagName = "query";
            Namespace = Uri.IQ_BROWSE;
        }

        public string Category
        {
            get { return GetAttribute("category"); }
            set { SetAttribute("category", value); }
        }

        public string Type
        {
            get { return GetAttribute("type"); }
            set { SetAttribute("type", value); }
        }

        public string Name
        {
            get { return GetAttribute("name"); }
            set { SetAttribute("name", value); }
        }

        public string[] GetNamespaces()
        {
            ElementList elements = SelectElements("ns");
            var nss = new string[elements.Count];

            int i = 0;
            foreach (Element ns in elements)
            {
                nss[i] = ns.Value;
                i++;
            }

            return nss;
        }

        public BrowseItem[] GetItems()
        {
            ElementList nl = SelectElements(typeof (BrowseItem));
            var items = new BrowseItem[nl.Count];
            int i = 0;
            foreach (Element item in nl)
            {
                items[i] = item as BrowseItem;
                i++;
            }
            return items;
        }
    }
}