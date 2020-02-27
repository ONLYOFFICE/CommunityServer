/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.InviteLink;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.UserForum;
using ASC.Web.Studio.UserControls.Common.VideoGuides;
using System;
using System.Web;

namespace ASC.Web.CRM.Controls.Common
{
    public partial class NavigationSidePanel : BaseUserControl
    {
        public static string Location
        {
            get
            {
                return PathProvider.GetFileStaticRelativePath("Common/NavigationSidePanel.ascx");
            }
        }

        protected string CurrentPage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitCurrentPage();
            var help = (HelpCenter)LoadControl(HelpCenter.Location);
            help.IsSideBar = true;
            HelpHolder.Controls.Add(help);
            SupportHolder.Controls.Add(LoadControl(Support.Location));
            VideoGuides.Controls.Add(LoadControl(VideoGuidesControl.Location));
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
            InviteUserHolder.Controls.Add(LoadControl(InviteLink.Location));
        }

        private void InitCurrentPage()
        {
            var currentPath = HttpContext.Current.Request.Path;
            if(currentPath.IndexOf("settings.aspx", StringComparison.Ordinal)>0)
            {
                var typeValue = (HttpContext.Current.Request["type"] ?? "common").ToLower();
                CurrentPage = "settings_" + typeValue;
            }
            else if (currentPath.IndexOf("cases.aspx", StringComparison.Ordinal) > 0)
            {
                CurrentPage = "cases";
            }
            else if (currentPath.IndexOf("deals.aspx", StringComparison.Ordinal) > 0)
            {
                CurrentPage = "deals";
            }
            else if (currentPath.IndexOf("tasks.aspx", StringComparison.Ordinal) > 0)
            {
                CurrentPage = "tasks";
            }
            else if (currentPath.IndexOf("help.aspx", StringComparison.Ordinal) > 0)
            {
                CurrentPage = "help";
            }
            else if (currentPath.IndexOf("invoices.aspx", StringComparison.Ordinal) > 0)
            {
                CurrentPage = "invoices";
            }
            else if (currentPath.IndexOf("mailviewer.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "contacts";
            }
            else if (currentPath.IndexOf("calls.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "calls";
            }
            else if (currentPath.IndexOf("reports.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "reports";
            }
            else
            {
                CurrentPage = "contacts";
                int contactID;
                if (int.TryParse(UrlParameters.ID, out contactID))
                {
                    var targetContact = DaoFactory.ContactDao.GetByID(contactID);
                    if (targetContact == null || !CRMSecurity.CanAccessTo(targetContact))
                        Response.Redirect(PathProvider.StartURL());
                    CurrentPage = targetContact is Company ? "companies" : "persons";
                }
            }

        }
    }
}