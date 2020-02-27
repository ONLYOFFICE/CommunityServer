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