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

using ASC.Core;
using System.Collections.Generic;

namespace ASC.SignalR.Base.Hubs.Chat
{
    public class ConnectionMapping
    {
        private readonly Dictionary<int, Dictionary<string, HashSet<string>>> _connections =
            new Dictionary<int, Dictionary<string, HashSet<string>>>();
        private readonly object _syncRoot = new object();

        public int Add(int tenantId, string userName, string connectionId)
        {
            lock (_syncRoot)
            {
                Dictionary<string, HashSet<string>> connectionsOfUser;
                HashSet<string> connectionsSet;
                if (!_connections.TryGetValue(tenantId, out connectionsOfUser))
                {
                    connectionsOfUser = new Dictionary<string, HashSet<string>>();
                    connectionsSet = new HashSet<string>();
                }
                else
                {
                    if (!connectionsOfUser.TryGetValue(userName, out connectionsSet))
                    {
                        connectionsSet = new HashSet<string>();
                    }
                }
                connectionsSet.Add(connectionId);
                connectionsOfUser[userName] = connectionsSet;
                _connections[tenantId] = connectionsOfUser;

                return connectionsSet.Count;
            }
        }

        public int Remove(int tenantId, string userName, string connectionId, out bool result)
        {
            lock (_syncRoot)
            {
                Dictionary<string, HashSet<string>> connectionsOfUser;
                HashSet<string> connectionsSet;
                result = false;
                if (_connections.TryGetValue(tenantId, out connectionsOfUser) && 
                    connectionsOfUser.TryGetValue(userName, out connectionsSet))
                {
                    result = connectionsSet.Remove(connectionId);
                    if (connectionsSet.Count != 0)
                    {
                        return connectionsSet.Count;
                    }
                    else
                    {
                        connectionsOfUser.Remove(userName);
                        if (connectionsOfUser.Count == 0)
                        {
                            _connections.Remove(tenantId);
                        }
                    }
                }
                return 0;
            }
        }

        public int GetConnectionsCount(int tenantId, string userName)
        {
            lock (_syncRoot)
            {
                Dictionary<string, HashSet<string>> connectionsOfUser;
                if (_connections.TryGetValue(tenantId, out connectionsOfUser))
                {
                    HashSet<string> connectionsSet;
                    if (connectionsOfUser.TryGetValue(userName, out connectionsSet))
                    {
                        return connectionsSet.Count;
                    }
                }
                return 0;
            }
        }

        public string GetUserNameByConnectionId(string connectionId, out int tenant)
        {
            lock (_syncRoot)
            {
                foreach (var tenantConnections in _connections)
                {
                    foreach (var userConnections in tenantConnections.Value)
                    {
                        foreach (var userConnectionId in userConnections.Value)
                        {
                            if (userConnectionId == connectionId)
                            {
                                tenant = tenantConnections.Key;
                                return userConnections.Key;
                            }
                        }
                    }
                }
                tenant = -1;
                return string.Empty;
            }
        }
    }
}
