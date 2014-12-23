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

namespace ASC.Xmpp.Core.protocol.iq.disco
{
    /*
	<iq type='result'
	from='plays.shakespeare.lit'
	to='romeo@montague.net/orchard'
	id='info1'>
	<query xmlns='http://jabber.org/protocol/disco#info'>
	<identity
	category='conference'
	type='text'
	name='Play-Specific Chatrooms'/>
	<identity
	category='directory'
	type='chatroom'
	name='Play-Specific Chatrooms'/>
	<feature var='http://jabber.org/protocol/disco#info'/>
	<feature var='http://jabber.org/protocol/disco#items'/>
	<feature var='http://jabber.org/protocol/muc'/>
	<feature var='jabber:iq:register'/>
	<feature var='jabber:iq:search'/>
	<feature var='jabber:iq:time'/>
	<feature var='jabber:iq:version'/>
	</query>
	</iq>
	*/

    /// <summary>
    ///   Summary description for DiscoIdentity.
    /// </summary>
    public class DiscoIdentity : Element
    {
        public DiscoIdentity()
        {
            TagName = "identity";
            Namespace = Uri.DISCO_INFO;
        }

        public DiscoIdentity(string type, string name, string category) : this()
        {
            Type = type;
            Name = name;
            Category = category;
        }

        public DiscoIdentity(string type, string category) : this()
        {
            Type = type;
            Category = category;
        }

        /// <summary>
        ///   type name for the entity
        /// </summary>
        public string Type
        {
            get { return GetAttribute("type"); }
            set { SetAttribute("type", value); }
        }

        /// <summary>
        ///   natural-language name for the entity
        /// </summary>
        public string Name
        {
            get { return GetAttribute("name"); }
            set { SetAttribute("name", value); }
        }

        /// <summary>
        ///   category name for the entity
        /// </summary>
        public string Category
        {
            get { return GetAttribute("category"); }
            set { SetAttribute("category", value); }
        }
    }
}