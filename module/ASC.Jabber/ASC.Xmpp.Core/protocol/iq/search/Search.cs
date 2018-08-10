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


using ASC.Xmpp.Core.protocol.x.data;
using ASC.Xmpp.Core.utils.Xml.Dom;

//	Example 1. Requesting Search Fields
//
//	<iq type='get'
//		from='romeo@montague.net/home'
//		to='characters.shakespeare.lit'
//		id='search1'
//		xml:lang='en'>
//		<query xmlns='jabber:iq:search'/>
//	</iq>
//

//	The service MUST then return the possible search fields to the user, and MAY include instructions:
//
//	Example 2. Receiving Search Fields
//
//	<iq type='result'
//		from='characters.shakespeare.lit'
//		to='romeo@montague.net/home'
//		id='search1'
//		xml:lang='en'>
//		<query xmlns='jabber:iq:search'>
//			<instructions>
//			Fill in one or more fields to search
//			for any matching Jabber users.
//			</instructions>
//			<first/>
//			<last/>
//			<nick/>
//			<email/>
//		</query>
//	</iq>    

namespace ASC.Xmpp.Core.protocol.iq.search
{
    /// <summary>
    ///   http://www.jabber.org/jeps/jep-0055.html
    /// </summary>
    public class Search : Element
    {
        public Search()
        {
            TagName = "query";
            Namespace = Uri.IQ_SEARCH;
        }

        public string Instructions
        {
            get { return GetTag("instructions"); }
            set { SetTag("instructions", value); }
        }

        public string Firstname
        {
            get { return GetTag("first"); }
            set { SetTag("first", value); }
        }

        public string Lastname
        {
            get { return GetTag("last"); }
            set { SetTag("last", value); }
        }

        public string Nickname
        {
            get { return GetTag("nick"); }
            set { SetTag("nick", value); }
        }

        public string Email
        {
            get { return GetTag("email"); }
            set { SetTag("email", value); }
        }

        /// <summary>
        ///   The X-Data Element
        /// </summary>
        public Data Data
        {
            get { return SelectSingleElement(typeof (Data)) as Data; }
            set
            {
                if (HasTag(typeof (Data)))
                    RemoveTag(typeof (Data));

                if (value != null)
                    AddChild(value);
            }
        }

        /// <summary>
        ///   Retrieve the result items of a search
        /// </summary>
        //public ElementList GetItems
        //{
        //    get
        //    {
        //        return this.SelectElements("item");
        //    }			
        //}
        public SearchItem[] GetItems()
        {
            ElementList nl = SelectElements(typeof (SearchItem));
            var items = new SearchItem[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (SearchItem) e;
                i++;
            }
            return items;
        }
    }
}