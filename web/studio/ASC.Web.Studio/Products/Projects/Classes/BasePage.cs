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
