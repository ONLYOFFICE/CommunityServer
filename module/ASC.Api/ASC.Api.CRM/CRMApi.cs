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


using System;
using System.Web;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;

namespace ASC.Api.CRM
{
    public partial class CRMApi : CRMApiBase, IApiEntryPoint
    {
        private readonly ApiContext _context;

        ///<summary>
        /// Api name entry
        ///</summary>
        public string Name
        {
            get { return "crm"; }
        }


        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="context"></param>
        public CRMApi(ApiContext context)
        {
            _context = context;
        }

        private static HttpRequest Request
        {
            get { return HttpContext.Current.Request; }
        }
        
        private static EntityType ToEntityType(string entityTypeStr)
        {
            EntityType entityType;

            if (string.IsNullOrEmpty(entityTypeStr)) return EntityType.Any;

            switch (entityTypeStr.ToLower())
            {
                case "person":
                    entityType = EntityType.Person;
                    break;
                case "company":
                    entityType = EntityType.Company;
                    break;
                case "contact":
                    entityType = EntityType.Contact;
                    break;
                case "opportunity":
                    entityType = EntityType.Opportunity;
                    break;
                case "case":
                    entityType = EntityType.Case;
                    break;
                default:
                    entityType = EntityType.Any;
                    break;
            }

            return entityType;
        }

        private string GetEntityTitle(EntityType entityType, int entityId, bool checkAccess, out DomainObject entity)
        {
            switch (entityType)
            {
                case EntityType.Contact:
                case EntityType.Company:
                case EntityType.Person:
                    var conatct = (entity = DaoFactory.ContactDao.GetByID(entityId)) as Contact;
                    if (conatct == null || (checkAccess && !CRMSecurity.CanAccessTo(conatct)))
                        throw new ItemNotFoundException();
                    return conatct.GetTitle();
                case EntityType.Opportunity:
                    var deal = (entity = DaoFactory.DealDao.GetByID(entityId)) as Deal;
                    if (deal == null || (checkAccess && !CRMSecurity.CanAccessTo(deal)))
                        throw new ItemNotFoundException();
                    return deal.Title;
                case EntityType.Case:
                    var cases = (entity = DaoFactory.CasesDao.GetByID(entityId)) as Cases;
                    if (cases == null || (checkAccess && !CRMSecurity.CanAccessTo(cases)))
                        throw new ItemNotFoundException();
                    return cases.Title;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }
    }
}