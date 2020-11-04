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