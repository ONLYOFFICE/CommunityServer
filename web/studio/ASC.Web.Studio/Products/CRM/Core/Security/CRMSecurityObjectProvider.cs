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
        private readonly DaoFactory _daoFactory;

        public CRMSecurityObjectProvider(DaoFactory daoFactory)
        {
            _daoFactory = daoFactory;
        }

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
                
                //   return _daoFactory.GetContactDao().GetByID(contactId);

            switch (entityType)
            {

                case EntityType.Opportunity:
                    return new Deal
                        {
                            ID = entityId,
                            Title = "fakeDeal"
                        };
                   // return _daoFactory.GetDealDao().GetByID(entityId);
                case EntityType.Case:
                    return new Cases
                        {
                            ID = entityId, 
                            Title = "fakeCases"
                        };
                  //  return _daoFactory.GetCasesDao().GetByID(entityId);
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