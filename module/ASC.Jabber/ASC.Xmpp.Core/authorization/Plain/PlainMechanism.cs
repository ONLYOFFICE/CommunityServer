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