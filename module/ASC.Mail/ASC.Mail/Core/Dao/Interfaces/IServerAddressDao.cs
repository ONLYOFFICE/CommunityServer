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


using System.Collections.Generic;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface IServerAddressDao
    {
        ServerAddress Get(int id);
        List<ServerAddress> GetList(List<int> ids = null);
        List<ServerAddress> GetList(int mailboxId);
        List<ServerAddress> GetGroupAddresses(int groupId);
        List<ServerAddress> GetDomainAddresses(int domainId);
        void AddAddressesToMailGroup(int groupId, List<int> addressIds);
        void DeleteAddressFromMailGroup(int groupId, int addressId);
        void DeleteAddressesFromMailGroup(int groupId);
        void DeleteAddressesFromAnyMailGroup(List<int> addressIds);
        int Save(ServerAddress address);
        int Delete(int id);
        int Delete(List<int> ids);
        bool IsAddressAlreadyRegistered(string addressName, string domainName);
    }
}
