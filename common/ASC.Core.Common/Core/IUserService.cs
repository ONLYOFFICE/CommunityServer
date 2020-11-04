/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using ASC.Core.Users;

namespace ASC.Core
{
    public interface IUserService
    {
        IDictionary<Guid, UserInfo> GetUsers(int tenant, DateTime from);

        UserInfo GetUser(int tenant, Guid id);

        UserInfo GetUserByPasswordHash(int tenant, string login, string passwordHash);

        UserInfo SaveUser(int tenant, UserInfo user);

        void RemoveUser(int tenant, Guid id);

        byte[] GetUserPhoto(int tenant, Guid id);

        void SetUserPhoto(int tenant, Guid id, byte[] photo);

        DateTime GetUserPasswordStamp(int tenant, Guid id);

        void SetUserPasswordHash(int tenant, Guid id, string passwordHash);


        IDictionary<Guid, Group> GetGroups(int tenant, DateTime from);

        Group GetGroup(int tenant, Guid id);

        Group SaveGroup(int tenant, Group group);

        void RemoveGroup(int tenant, Guid id);


        IDictionary<string, UserGroupRef> GetUserGroupRefs(int tenant, DateTime from);

        UserGroupRef SaveUserGroupRef(int tenant, UserGroupRef r);

        void RemoveUserGroupRef(int tenant, Guid userId, Guid groupId, UserGroupRefType refType);
    }
}
