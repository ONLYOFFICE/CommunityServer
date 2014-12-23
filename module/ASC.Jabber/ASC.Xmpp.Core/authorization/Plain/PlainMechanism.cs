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

#region using

using System;
using System.Text;
using ASC.Xmpp.Core.utils.Xml.Dom;

#endregion

namespace ASC.Xmpp.Core.authorization.Plain
{
    /// <summary>
    ///   Summary description for PlainMechanism.
    /// </summary>
    public class PlainMechanism : Mechanism
    {
        #region Members

        //private XmppClientConnection m_XmppClient = null;

        #endregion

        #region Constructor

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="con"> </param>
        public override void Init()
        {
            // m_XmppClient = con;

            // <auth mechanism="PLAIN" xmlns="urn:ietf:params:xml:ns:xmpp-sasl">$Message</auth>
            //m_XmppClient.Send(new Auth(MechanismType.PLAIN, Message()));
        }

        /// <summary>
        /// </summary>
        /// <param name="e"> </param>
        public override void Parse(Node e)
        {
            // not needed here in PLAIN mechanism
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        private string Message()
        {
            // NULL Username NULL Password
            var sb = new StringBuilder();

            // sb.Append( (char) 0 );
            // sb.Append(this.m_XmppClient.MyJID.Bare);
            sb.Append((char) 0);
            sb.Append(Username);
            sb.Append((char) 0);
            sb.Append(Password);

            byte[] msg = Encoding.UTF8.GetBytes(sb.ToString());
            return Convert.ToBase64String(msg, 0, msg.Length);
        }

        #endregion
    }
}