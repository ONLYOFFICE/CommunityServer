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
