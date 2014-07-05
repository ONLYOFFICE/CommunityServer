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
// // <copyright company="Ascensio System Limited" file="Auth.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

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