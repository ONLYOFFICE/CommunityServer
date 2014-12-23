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