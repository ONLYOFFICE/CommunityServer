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
using System.Runtime.Remoting.Messaging;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Masters;
using ASC.Web.Studio;
using Global = ASC.Web.Projects.Classes.Global;

namespace ASC.Web.Projects
{
    public abstract class BasePage : MainPage
    {
        #region Properties

        public Participant Participant { get; private set; }

        public Project Project { get; set; }

        public string EssenceTitle { get; set; }

        public string EssenceStatus { get; set; }

        public bool IsSubcribed { get; set; }

        protected virtual bool CheckSecurity { get { return true; } }

        protected virtual bool CanRead { get { return true; } }

        #endregion

        protected BasePage()
        {
            PreInit += PagePreInit;
        }

        protected void PagePreInit(object sender, EventArgs e)
        {
            if (CallContext.GetData("CURRENT_ACCOUNT") == null && SecurityContext.IsAuthenticated)
            {
                CallContext.SetData("CURRENT_ACCOUNT", SecurityContext.CurrentAccount.ID);
            }

            if (!SecurityContext.IsAuthenticated) return;

            Participant = Global.EngineFactory.GetParticipantEngine().GetByID(SecurityContext.CurrentAccount.ID);
            Participant.IsAdmin = WebItemSecurity.IsProductAdministrator(EngineFactory.ProductId, SecurityContext.CurrentAccount.ID);

            if (RequestContext.IsInConcreteProject)
            {
                Project = RequestContext.GetCurrentProject(false);

                if (Project == null)
                {
                    RedirectNotFound("projects.aspx");
                    return;
                }

                if (!CanRead)
                {
                    Response.Redirect("projects.aspx?prjID=" + Project.ID, true);
                }

                EssenceTitle = Project.Title;
                EssenceStatus = Project.Status != ProjectStatus.Open ? LocalizedEnumConverter.ConvertToString(Project.Status).ToLower() : "";

                if (!RequestContext.IsInConcreteProjectModule)
                {
                    IsSubcribed = Global.EngineFactory.GetProjectEngine().IsFollow(Project.ID, Participant.ID);
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
    }
}
