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
using ASC.Core.Common.Notify.Jabber;
using System;
using System.Collections.Generic;

namespace ASC.Core.Notify.Jabber
{
    public class JabberServiceClientWcf : BaseWcfClient<IJabberService>, IJabberService, IDisposable
    {
        public JabberServiceClientWcf()
        {
        }

        public byte AddXmppConnection(string connectionId, string userName, byte state, int tenantId)
        {
            return Channel.AddXmppConnection(connectionId, userName, state, tenantId);
        }

        public byte RemoveXmppConnection(string connectionId, string userName, int tenantId)
        {
            return Channel.RemoveXmppConnection(connectionId, userName, tenantId);
        }

        public int GetNewMessagesCount(int tenantId, string userName)
        {
            return Channel.GetNewMessagesCount(tenantId, userName);
        }

        public string GetUserToken(int tenantId, string userName)
        {
            return Channel.GetUserToken(tenantId, userName);
        }

        public void SendCommand(int tenantId, string from, string to, string command, bool fromTenant)
        {
            Channel.SendCommand(tenantId, from, to, command, fromTenant);
        }

        public void SendMessage(int tenantId, string from, string to, string text, string subject)
        {
            Channel.SendMessage(tenantId, from, to, text, subject);
        }

        public byte SendState(int tenantId, string userName, byte state)
        {
            return Channel.SendState(tenantId, userName, state);
        }

        public MessageClass[] GetRecentMessages(int tenantId, string from, string to, int id)
        {
            return Channel.GetRecentMessages(tenantId, from, to, id);
        }

        public Dictionary<string, byte> GetAllStates(int tenantId, string userName)
        {
            return Channel.GetAllStates(tenantId, userName);
        }

        public byte GetState(int tenantId, string userName)
        {
            return Channel.GetState(tenantId, userName);
        }

        public void Ping(string userId, int tenantId, string userName, byte state)
        {
            Channel.Ping(userId, tenantId, userName, state);
        }

        public string HealthCheck(string userName, int tenantId)
        {
            return Channel.HealthCheck(userName, tenantId);
        }
    }
}
