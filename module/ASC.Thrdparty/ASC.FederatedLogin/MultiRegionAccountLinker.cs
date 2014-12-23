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

#region Import

using System;
using System.Collections.Generic;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.FederatedLogin.Profile;
using System.Linq;
using System.Linq.Expressions;
using System.Configuration;

#endregion


namespace ASC.FederatedLogin
{

    public class MultiRegionAccountLinker
    {

        #region Members

        private readonly Dictionary<String, AccountLinker> _accountLinkers = new Dictionary<String, AccountLinker>();
        private readonly String _baseDatabaseId = null;

        #endregion

        private String GetDatabaseId(String hostedRegion)
        {
            var databaseId = _baseDatabaseId;

            if (!String.IsNullOrEmpty(hostedRegion))
                databaseId = String.Join(".", new[] { _baseDatabaseId, hostedRegion.Trim() });

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
            _accountLinkers[GetDatabaseId(region)].Unlink(obj);
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