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


using ASC.FederatedLogin.Profile;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ASC.FederatedLogin
{
    public class MultiRegionAccountLinker
    {
        private readonly Dictionary<String, AccountLinker> _accountLinkers = new Dictionary<String, AccountLinker>();
        private readonly String _baseDatabaseId = null;

        private String GetDatabaseId(String hostedRegion)
        {
            var databaseId = _baseDatabaseId;

            if (!String.IsNullOrEmpty(hostedRegion))
                databaseId = String.Join(".", new[] {_baseDatabaseId, hostedRegion.Trim()});

            if (!_accountLinkers.ContainsKey(databaseId))
                throw new ArgumentException(String.Format("Region {0} is not defined", databaseId), "hostedRegion");

            return databaseId;
        }


        public MultiRegionAccountLinker(String databaseId)
        {
            foreach (ConnectionStringSettings connection in ConfigurationManager.ConnectionStrings)
            {
                if (connection.Name.StartsWith(databaseId))
                    _accountLinkers.Add(connection.Name, new AccountLinker(connection.Name));
            }
        }

        public IEnumerable<String> GetLinkedObjects(string id, string provider)
        {
            return _accountLinkers.Values.SelectMany(x => x.GetLinkedObjects(id, provider));
        }

        public IEnumerable<String> GetLinkedObjects(LoginProfile profile)
        {
            return _accountLinkers.Values.SelectMany(x => x.GetLinkedObjects(profile));
        }

        public IEnumerable<String> GetLinkedObjectsByHashId(string hashid)
        {
            return _accountLinkers.Values.SelectMany(x => x.GetLinkedObjectsByHashId(hashid));
        }

        public void AddLink(String hostedRegion, string obj, LoginProfile profile)
        {
            _accountLinkers[GetDatabaseId(hostedRegion)].AddLink(obj, profile);
        }

        public void AddLink(String hostedRegion, string obj, string id, string provider)
        {
            _accountLinkers[GetDatabaseId(hostedRegion)].AddLink(obj, id, provider);
        }

        public void RemoveLink(String hostedRegion, string obj, string id, string provider)
        {
            _accountLinkers[GetDatabaseId(hostedRegion)].RemoveLink(obj, id, provider);
        }

        public void RemoveLink(String hostedRegion, string obj, LoginProfile profile)
        {
            _accountLinkers[GetDatabaseId(hostedRegion)].RemoveLink(obj, profile);
        }

        public void Unlink(String region, string obj)
        {
            _accountLinkers[GetDatabaseId(region)].RemoveProvider(obj);
        }

        public void RemoveProvider(String hostedRegion, string obj, string provider)
        {
            _accountLinkers[GetDatabaseId(hostedRegion)].RemoveProvider(obj, provider);
        }

        public IEnumerable<LoginProfile> GetLinkedProfiles(string obj)
        {
            return _accountLinkers.Values.SelectMany(x => x.GetLinkedProfiles(obj));
        }
    }
}