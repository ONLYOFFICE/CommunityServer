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


#region Import

using System;
using System.Collections.Generic;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using Action = ASC.Common.Security.Authorizing.Action;
#endregion

namespace ASC.CRM.Core
{
    public class CRMSecurityObjectProvider : ISecurityObjectProvider
    {
        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            int contactId;
            int entityId;
            EntityType entityType;

            if (objectId is Task)
            {
                var task = (Task)objectId;

                contactId = task.ContactID;
                entityId = task.EntityID;
                entityType = task.EntityType;
            }
            else if (objectId is RelationshipEvent)
            {
                var eventObj = (RelationshipEvent)objectId;

                contactId = eventObj.ContactID;
                entityId = eventObj.EntityID;
                entityType = eventObj.EntityType;
                
            }
            else
            {
                return null;
            }

            if (entityId == 0 && contactId == 0) return null;

            if (entityId == 0)
            return new Company
                {
                    ID = contactId,
                    CompanyName = "fakeCompany"
                };
                
                //   return _daoFactory.ContactDao.GetByID(contactId);

            switch (entityType)
            {

                case EntityType.Opportunity:
                    return new Deal
                        {
                            ID = entityId,
                            Title = "fakeDeal"
                        };
                   // return _daoFactory.DealDao.GetByID(entityId);
                case EntityType.Case:
                    return new Cases
                        {
                            ID = entityId, 
                            Title = "fakeCases"
                        };
                  //  return _daoFactory.CasesDao.GetByID(entityId);
            }

            return null;
        }

        public bool InheritSupported
        {
            get { return true; }
        }

        public bool ObjectRolesSupported
        {
            get { return false; }
        }

        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
 
          //   Constants.Everyone
          // if (_daoFactory.GetManagerDao().GetAll(false).Contains(ASC.Core.CoreContext.UserManager.GetUsers(account.ID)))
          //   return new Action[]
            throw new NotImplementedException();
        }
    }
}