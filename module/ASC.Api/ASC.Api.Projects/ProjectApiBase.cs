/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System;
using System.Runtime.Remoting.Messaging;
using ASC.Api.Impl;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Core;
using ASC.Projects.Engine;

namespace ASC.Api.Projects
{
    public class ProjectApiBase
    {
        internal const string DbId = "projects"; //Copied from projects
        protected ApiContext _context;
        private EngineFactory _engineFactory;

        protected EngineFactory EngineFactory
        {
            get
            {
                if (_engineFactory == null)
                {
                    _engineFactory = new EngineFactory(DbId, TenantId);
                }
                //NOTE: don't sure if it's need to be here since remoting is gone
                if (CallContext.GetData("CURRENT_ACCOUNT") == null && SecurityContext.IsAuthenticated)
                {
                    CallContext.SetData("CURRENT_ACCOUNT", SecurityContext.CurrentAccount.ID);
                }
                return _engineFactory;
            }
        }

        protected DaoFactory CrmDaoFactory
        {
            get { return new DaoFactory(TenantId, CRMConstants.DatabaseId); }
        }

        private static int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        protected static Guid CurrentUserId
        {
            get { return SecurityContext.CurrentAccount.ID; }
        }
    }
}