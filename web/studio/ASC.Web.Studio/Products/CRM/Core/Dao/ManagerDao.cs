/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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