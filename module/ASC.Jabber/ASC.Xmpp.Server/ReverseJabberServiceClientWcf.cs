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

using ASC.Common.Module;
using ASC.Core.Notify.Jabber;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ASC.Xmpp.Server
{
    public class ReverseJabberServiceClientWcf : BaseWcfClient<IReverseJabberService>, IReverseJabberService, IDisposable
    {
        private const string TOKEN = "token";
        private readonly string token = ConfigurationManager.AppSettings["web.chat-token"] ?? "95739c2e-e001-4b50-a179-9950678b2bb0";

        public void SendMessage(string callerUserName, string calleeUserName, string messageText, int tenantId, string domain)
        {
            using (var scope = new OperationContextScope(InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(TOKEN, string.Empty, token));
                Channel.SendMessage(callerUserName, calleeUserName, messageText, tenantId, domain);
            }
        }

        public void SendInvite(string chatRoomName, string calleeUserName, string domain)
        {
            using (var scope = new OperationContextScope(InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(TOKEN, string.Empty, token));
                Channel.SendInvite(chatRoomName, calleeUserName, domain);
            }
        }

        public void SendState(string from, byte state, int tenantId, string domain)
        {
            using (var scope = new OperationContextScope(InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(TOKEN, string.Empty, token));
                Channel.SendState(from, state, tenantId, domain);
            }
        }

        public void SendOfflineMessages(string callerUserName, List<string> users, int tenantId)
        {
            using (var scope = new OperationContextScope(InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(TOKEN, string.Empty, token));
                Channel.SendOfflineMessages(callerUserName, users, tenantId);
            }
        }
    }
}
