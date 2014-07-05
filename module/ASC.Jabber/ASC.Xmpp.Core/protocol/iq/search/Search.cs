/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="Search.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

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