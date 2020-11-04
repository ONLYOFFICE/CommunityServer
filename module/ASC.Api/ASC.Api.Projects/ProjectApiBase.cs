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
using Autofac;

namespace ASC.Api.Projects
{
    public class ProjectApiBase: IDisposable
    {
        protected internal ApiContext Context;
        private readonly ILifetimeScope scope;
        private readonly ILifetimeScope crmScope;

        protected internal ProjectSecurity ProjectSecurity { get; private set; }
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
            ProjectSecurity = scope.Resolve<ProjectSecurity>();

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