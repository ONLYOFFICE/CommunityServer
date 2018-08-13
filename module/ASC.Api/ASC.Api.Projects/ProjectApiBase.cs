/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections.Generic;
using ASC.Api.Documents;
using ASC.Api.Employee;
using ASC.Api.Impl;
using ASC.Api.Projects.Wrappers;
using ASC.Collections;
using ASC.CRM.Core.Dao;
using ASC.Core;
using ASC.Files.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Projects.Core;
using ASC.Web.Studio.UserControls.Common.Comments;
using Autofac;

namespace ASC.Api.Projects
{
    public class ProjectApiBase: IDisposable
    {
        protected internal ApiContext Context;
        private readonly ILifetimeScope scope;
        private readonly ILifetimeScope crmScope;
        
        protected EngineFactory EngineFactory { get; private set; }
        protected DaoFactory CrmDaoFactory { get; private set; }
        private readonly HttpRequestDictionary<EmployeeWraperFull> employeeFullCache = new HttpRequestDictionary<EmployeeWraperFull>("employeeFullCache");
        private readonly HttpRequestDictionary<EmployeeWraper> employeeCache = new HttpRequestDictionary<EmployeeWraper>("employeeCache");

        protected Func<Task, TaskWrapper> TaskWrapperSelector;
        protected Func<Message, MessageWrapper> MessageWrapperSelector;
        protected Func<Milestone, MilestoneWrapper> MilestoneWrapperSelector;
        protected internal Func<Project, ProjectWrapper> ProjectWrapperSelector;
        protected internal Func<Project,object, ProjectWrapperFull> ProjectWrapperFullSelector;
        protected internal Func<File, FileWrapper> FileWrapperSelector;
        protected internal Func<TimeSpend, TimeWrapper> TimeWrapperSelector;

        public ProjectApiBase()
        {
            scope = DIHelper.Resolve();
            EngineFactory = scope.Resolve<EngineFactory>();

            crmScope = Web.CRM.Core.DIHelper.Resolve();
            CrmDaoFactory = crmScope.Resolve<DaoFactory>();

            TaskWrapperSelector = r => new TaskWrapper(this, r);
            MessageWrapperSelector = r => new MessageWrapper(this, r);
            MilestoneWrapperSelector = r => new MilestoneWrapper(this, r);
            ProjectWrapperSelector = r => new ProjectWrapper(this, r);
            ProjectWrapperFullSelector = (p,f) => new ProjectWrapperFull(this, p, f);
            FileWrapperSelector = x => new FileWrapper(x);
            TimeWrapperSelector = x => new TimeWrapper(this, x);
        }

        protected static Guid CurrentUserId
        {
            get { return SecurityContext.CurrentAccount.ID; }
        }

        public EmployeeWraperFull GetEmployeeWraperFull(Guid userId)
        {
            return employeeFullCache.Get(userId.ToString(), () => EmployeeWraperFull.GetFull(CoreContext.UserManager.GetUsers(userId)));
        }

        public EmployeeWraper GetEmployeeWraper(Guid userId)
        {
            var employee = employeeFullCache.Get(userId.ToString());
            return employee ??
                   employeeCache.Get(userId.ToString(), () => EmployeeWraper.Get(CoreContext.UserManager.GetUsers(userId)));
        }

        public void Dispose()
        {
            if (scope != null)
            {
                scope.Dispose();
            }

            if (crmScope != null)
            {
                crmScope.Dispose();
            }
        }
    }
}