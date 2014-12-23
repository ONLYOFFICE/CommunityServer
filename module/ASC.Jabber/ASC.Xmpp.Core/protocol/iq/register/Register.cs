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

        #endregion
    }
}