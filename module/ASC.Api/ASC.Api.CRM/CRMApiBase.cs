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
using ASC.Common.Web;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Files.Core;
using ASC.VoipService;
using ASC.VoipService.Dao;
using ASC.Web.Projects.Core;
using Autofac;
using FilesGlobal = ASC.Web.Files.Classes.Global;

namespace ASC.Api.CRM
{
    public class CRMApiBase : IDisposable
    {
        private IDaoFactory filesDaoFactory;

        private readonly ILifetimeScope scope;
        private readonly ILifetimeScope crmScope;

        public CRMApiBase()
        {
            scope = DIHelper.Resolve();
            ProjectsDaoFactory = scope.Resolve<Projects.Core.DataInterfaces.IDaoFactory>();

            crmScope = Web.CRM.Core.DIHelper.Resolve();
            DaoFactory = crmScope.Resolve<DaoFactory>();
        }

        protected DaoFactory DaoFactory { get; private set; }

        protected IVoipProvider VoipProvider
        {
            get { return VoipDao.GetProvider(); }
        }

        protected Projects.Core.DataInterfaces.IDaoFactory ProjectsDaoFactory { get; private set; }

        protected IDaoFactory FilesDaoFactory
        {
            get { return filesDaoFactory ?? (filesDaoFactory = FilesGlobal.DaoFactory); }
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