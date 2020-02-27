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


using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.iq.auth
{
    //	Send:<iq type='get' to='myjabber.net' id='MX_7'>
    //			<query xmlns='jabber:iq:auth'><username>gnauck</username></query>
    //		 </iq>
    //	Recv:<iq type="result" id="MX_7"><query xmlns="jabber:iq:auth"><username>gnauck</username><password/><digest/><resource/></query></iq> 
    //
    //	Send:<iq type='set' id='mx_login'><query xmlns='jabber:iq:auth'><username>gnauck</username><digest>27c05d464e3f908db3b2ca1729674bfddb28daf2</digest><resource>Office</resource></query></iq>
    //	Recv:<iq id="mx_login" type="result"/> 


    ///<summary>
    ///</summary>
    public class Auth : Element
    {
        #region << Constructors >>

        public Auth()
        {
            TagName = "query";
            Namespace = Uri.IQ_AUTH;
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

        public string Resource
        {
            get { return GetTag("resource"); }
            set { SetTag("resource", value); }
        }

        public string Digest
        {
            get { return GetTag("digest"); }
            set { SetTag("digest", value); }
        }

        #endregion

        #region << Public Methods >>

        /// <summary>
        /// </summary>
        /// <param name="username"> </param>
        /// <param name="password"> </param>
        /// <param name="StreamID"> </param>
        public void SetAuthDigest(string username, string password, string StreamID)
        {
            // Jive Messenger has a problem when we dont remove the password Tag
            RemoveTag("password");
            Username = username;
            Digest = Hash.Sha1Hash(StreamID + password);
        }

        /// <summary>
        /// </summary>
        /// <param name="username"> </param>
        /// <param name="password"> </param>
        public void SetAuthPlain(string username, string password)
        {
            // remove digest Tag when existing
            RemoveTag("digest");
            Username = username;
            Password = password;
        }

        /// <summary>
        /// </summary>
        public void SetAuth(string username, string password, string streamId)
        {
            if (HasTag("digest"))
                SetAuthDigest(username, password, streamId);
            else
                SetAuthPlain(username, password);
        }

        #endregion
    }
}