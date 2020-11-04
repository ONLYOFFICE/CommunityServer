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


#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.CRM.Core.Entities;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Configuration;
using ASC.Web.Files.Api;
using ASC.Web.Studio.Helpers;
using ASC.Web.Studio.Utility;
using log4net;

#endregion

namespace ASC.CRM.Core.Dao
{

    public class ManagerDao : AbstractDao
    {

        #region Constructor

        public ManagerDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {


        }

        #endregion

        #region Methods

        public void Add(Guid managerID)
        {
            DbManager.ExecuteNonQuery(Insert("crm_manager").InColumnValue("id", managerID));

        }

        public void Remove(Guid managerID)
        {

            DbManager.ExecuteNonQuery(Delete("crm_manager").Where(Exp.Eq("id", managerID)));

        }

        public List<UserInfo> GetAll(bool includeAdmins)
        {

            var managers = DbManager.ExecuteList(Query("crm_manager").Select("id")).ConvertAll(row=> ASC.Core.CoreContext.UserManager.GetUsers(ToGuid(row[0])));
           
            if (includeAdmins)
              return managers.Union(ASC.Core.CoreContext.UserManager.GetUsersByGroup(Constants.GroupAdmin.ID)).Distinct().ToList();

            return managers;
        }

        #endregion

    }

}