/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
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

        private string GetEntityTitle(EntityType entityType, int entityId)
        {
            switch (entityType)
            {
                case EntityType.Contact:
                    return DaoFactory.GetContactDao().GetByID(entityId).GetTitle();
                case EntityType.Company:
                    return DaoFactory.GetContactDao().GetByID(entityId).GetTitle();
                case EntityType.Person:
                    return DaoFactory.GetContactDao().GetByID(entityId).GetTitle();
                case EntityType.Opportunity:
                    return DaoFactory.GetDealDao().GetByID(entityId).Title;
                case EntityType.Case:
                    return DaoFactory.GetCasesDao().GetByID(entityId).Title;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }
    }
}