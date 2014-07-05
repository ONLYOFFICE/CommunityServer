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

using ASC.Core;
using System.Collections.Generic;

namespace ASC.SignalR.Base.Hubs.Chat
{
    public class UserStateMapping
    {
        private readonly Dictionary<int, Dictionary<string, byte>> states =
            new Dictionary<int, Dictionary<string, byte>>(CoreContext.TenantManager.GetTenants().Count);

        public readonly object SyncRoot = new object();

        // Non thread safe
        public Dictionary<int, Dictionary<string, byte>> States
        { 
            get 
            { 
                return states; 
            }
        }

        public void Add(int tenantId, string userName, byte state)
        {
            lock (SyncRoot)
            {
                Dictionary<string, byte> statesOfUsers;
                if (!states.TryGetValue(tenantId, out statesOfUsers)) 
                {
                    statesOfUsers = new Dictionary<string, byte>();
                    states.Add(tenantId, statesOfUsers);
                }
                statesOfUsers[userName] = state;
            }
        }

        public void Remove(int tenantId, string userName)
        {
            lock (SyncRoot)
            {
                Dictionary<string, byte> statesOfUsers;
                if (!states.TryGetValue(tenantId, out statesOfUsers)) 
                {
                    return;
                }
                statesOfUsers.Remove(userName);
                if (statesOfUsers.Count == 0)
                {
                    states.Remove(tenantId);
                }
            }
        }

        public Dictionary<string, byte> GetStatesOfTenant(int tenantId)
        {
            Dictionary<string, byte> statesOfTenant;
            lock (SyncRoot)
            {
                if (states.TryGetValue(tenantId, out statesOfTenant))
                {
                    return statesOfTenant;
                }
                statesOfTenant = new Dictionary<string, byte>();
                states.Add(tenantId, statesOfTenant);
            }
            return statesOfTenant;
        }

        public byte GetState(int tenantId, string userName)
        {
            byte state;
            lock (SyncRoot)
            {
                Dictionary<string, byte> statesOfUsers;
                if (!states.TryGetValue(tenantId, out statesOfUsers) || !statesOfUsers.TryGetValue(userName, out state))
                {
                    return Chat.UserOffline;
                }
            }
            return state;
        }
    }
}