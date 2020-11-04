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
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Masters;
using ASC.Web.Studio;
using ASC.Web.Projects.Core;
using Autofac;

namespace ASC.Web.Projects
{
    public abstract class BasePage : MainPage
    {
        #region Properties

        public Participant Participant { get; private set; }

        public RequestContext RequestContext { get; private set; }

        public Project Project { get; set; }

        public EngineFactory EngineFactory { get; private set; }

        public ProjectSecurity ProjectSecurity { get; private set; }

        protected virtual bool CheckSecurity { get { return true; } }

        protected virtual bool CanRead { get { return true; } }

        protected ILifetimeScope Scope { get; set; }

        #endregion

        protected BasePage()
        {
            Scope = DIHelper.Resolve();
            PreInit += PagePreInit;
            EngineFactory = Scope.Resolve<EngineFactory>();
            RequestContext = new RequestContext(EngineFactory);
            ProjectSecurity = Scope.Resolve<ProjectSecurity>();
        }

        protected void PagePreInit(object sender, EventArgs e)
        {
            if (!SecurityContext.IsAuthenticated) return;

            Participant = EngineFactory.ParticipantEngine.GetByID(SecurityContext.CurrentAccount.ID);

            if (RequestContext.IsInConcreteProject)
            {
                Project = RequestContext.GetCurrentProject(false);

                if (Project == null)
                {
                    RedirectNotFound("Projects.aspx");
                    return;
                }

                if (!CanRead)
                {
                    Response.Redirect("Projects.aspx?prjID=" + Project.ID, true);
                }
            }

            if (!CheckSecurity)
            {
                Response.Redirect(PathProvider.BaseVirtualPath, true);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            PageLoad();
        }

        protected abstract void PageLoad();

        public new BasicTemplate Master
        {
            get { return base.Master as BasicTemplate; }
        }

        public void RedirectNotFound(string url)
        {
            Response.Redirect(url + "#elementNotFound", true);
        }
        protected override void OnUnload(EventArgs e)
        {
            if (Scope != null)
            {
                Scope.Dispose();
            }
            base.OnUnload(e);
        }
    }
}
