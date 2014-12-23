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