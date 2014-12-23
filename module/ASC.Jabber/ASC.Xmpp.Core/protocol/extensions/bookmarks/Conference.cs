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

namespace ASC.Xmpp.Core.protocol.extensions.bookmarks
{
    /// <summary>
    ///   One of the most common uses of bookmarks will likely be to bookmark conference rooms on various Jabber servers
    /// </summary>
    public class Conference : Element
    {
        /*
             <iq type='result' id='2'>
               <query xmlns='jabber:iq:private'>
                 <storage xmlns='storage:bookmarks'>
                   <conference name='Council of Oberon' 
                               autojoin='true'
                               jid='council@conference.underhill.org'>
                     <nick>Puck</nick>
                     <password>titania</password>
                   </conference>
                 </storage>
               </query>
             </iq>   
         */

        public Conference()
        {
            TagName = "conference";
            Namespace = Uri.STORAGE_BOOKMARKS;
        }

        public Conference(Jid jid, string name) : this()
        {
            Jid = jid;
            Name = name;
        }

        public Conference(Jid jid, string name, string nickname) : this(jid, name)
        {
            Nickname = nickname;
        }

        public Conference(Jid jid, string name, string nickname, string password) : this(jid, name, nickname)
        {
            Password = password;
        }

        public Conference(Jid jid, string name, string nickname, string password, bool autojoin)
            : this(jid, name, nickname, password)
        {
            AutoJoin = autojoin;
        }

        /// <summary>
        ///   A name/description for this bookmarked room
        /// </summary>
        public string Name
        {
            get { return GetAttribute("name"); }
            set { SetAttribute("name", value); }
        }

        /// <summary>
        ///   Should the client join this room automatically after successfuil login?
        /// </summary>
        public bool AutoJoin
        {
            get { return GetAttributeBool("autojoin"); }
            set { SetAttribute("autojoin", value); }
        }

        /// <summary>
        ///   The Jid of the bookmarked room
        /// </summary>
        public Jid Jid
        {
            get { return GetAttributeJid("jid"); }
            set { SetAttribute("jid", value); }
        }

        /// <summary>
        ///   The Nickname for this room
        /// </summary>
        public string Nickname
        {
            get { return GetTag("nickname"); }
            set { SetTag("nickname", value); }
        }

        /// <summary>
        ///   The password for password protected rooms
        /// </summary>
        public string Password
        {
            get { return GetTag("password"); }
            set { SetTag("password", value); }
        }
    }
}