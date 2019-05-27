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

// Sample 1
// <SENT> <iq id="2" type="set"><query xmlns="jabber:iq:register"><username>gnauck2</username><password>secret</password></query></iq>
// <RECV> <iq id='2' type='result'/>

// Sample 2
// <SEND> <iq xmlns="jabber:client" id="agsXMPP_1" type="get" to="127.0.0.1"><query xmlns="jabber:iq:register"><username>test1</username><password>secret</password></query></iq>
// <RECV> <iq xmlns="jabber:client" id="agsXMPP_1" type="result"><query xmlns="jabber:iq:register"><username>test1</username><password>mysecret</password><password /><instructions>Choose a username and password to register with this server.</instructions><name /><email /><username /></query></iq>

namespace ASC.Xmpp.Core.protocol.iq.register
{
    /// <summary>
    ///   Used for registering new usernames on Jabber/XMPP Servers
    /// </summary>
    public class Register : Element
    {
        #region << Constructors >>

        public Register()
        {
            TagName = "query";
            Namespace = Uri.IQ_REGISTER;
        }

        public Register(string username, string password) : this()
        {
            Username = username;
            Password = password;
        }

        #endregion

        #region << Properties >>

        public string Username
        {
            get { return GetTag("username"); }
            set { SetTag("username", value); }
        }

        public string Password
        {
            get { return GetTag("password"); }
            set { SetTag("password", value); }
        }

        public string Instructions
        {
            get { return GetTag("instructions"); }
            set { SetTag("instructions", value); }
        }

        public string Name
        {
            get { return GetTag("name"); }
            set { SetTag("name", value); }
        }

        public string Email
        {
            get { return GetTag("email"); }
            set { SetTag("email", value); }
        }

        /// <summary>
        ///   Remove registration from the server
        /// </summary>
        public bool RemoveAccount
        {
            get { return HasTag("remove"); }
            set
            {
                if (value)
                    SetTag("remove");
                else
                    RemoveTag("remove");
            }
        }

        /// <summary>
        ///   The X-Data Element
        /// </summary>
        public x.data.Data Data
        {
            get { return SelectSingleElement(typeof (x.data.Data)) as x.data.Data; }
            set
            {
                if (HasTag(typeof (x.data.Data)))
                    RemoveTag(typeof (x.data.Data));

                if (value != null)
                    AddChild(value);
            }
        }

        #endregion
    }
}