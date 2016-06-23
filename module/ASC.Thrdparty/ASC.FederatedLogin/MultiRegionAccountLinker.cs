/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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