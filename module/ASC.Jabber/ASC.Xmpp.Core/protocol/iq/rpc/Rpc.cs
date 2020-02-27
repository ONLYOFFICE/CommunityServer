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

namespace ASC.Xmpp.Core.protocol.iq.rpc
{
    /*         

        Example 1. A typical request

        <iq type='set' to='responder@company-a.com/jrpc-server' id='1'>
          <query xmlns='jabber:iq:rpc'>
            <methodCall>
              <methodName>examples.getStateName</methodName>
              <params>
                <param>
                  <value><i4>6</i4></value>
                </param>
              </params>
            </methodCall>
          </query>
        </iq>

        Example 2. A typical response

        <iq type='result' to='requester@company-b.com/jrpc-client' 
                    from='responder@company-a.com/jrpc-server' id='1'>
          <query xmlns='jabber:iq:rpc'>
            <methodResponse>
              <params>
                <param>
                  <value><string>Colorado</string></value>
                </param>
              </params>
            </methodResponse>
          </query>
        </iq>

    */

    /// <summary>
    ///   JEP-0009: Jabber-RPC, transport RPC over Jabber/XMPP
    /// </summary>
    public class Rpc : Element
    {
        public Rpc()
        {
            TagName = "query";
            Namespace = Uri.IQ_RPC;
        }


        /// <summary>
        /// </summary>
        public MethodCall MethodCall
        {
            get { return (MethodCall) SelectSingleElement(typeof (MethodCall)); }
            set
            {
                RemoveTag(typeof (MethodCall));
                if (value != null)
                    AddChild(value);
            }
        }

        /// <summary>
        /// </summary>
        public MethodResponse MethodResponse
        {
            get { return (MethodResponse) SelectSingleElement(typeof (MethodResponse)); }
            set
            {
                RemoveTag(typeof (MethodResponse));
                if (value != null)
                    AddChild(value);
            }
        }
    }
}