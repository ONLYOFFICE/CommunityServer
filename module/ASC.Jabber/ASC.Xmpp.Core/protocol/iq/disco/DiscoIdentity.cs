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