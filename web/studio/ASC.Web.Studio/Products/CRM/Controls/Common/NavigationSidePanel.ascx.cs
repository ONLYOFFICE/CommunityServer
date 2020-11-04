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


using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.InviteLink;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.UserForum;
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
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
            InviteUserHolder.Controls.Add(LoadControl(InviteLink.Location));
        }

        private void InitCurrentPage()
        {
            var currentPath = HttpContext.Current.Request.Path;
            if(currentPath.IndexOf("Settings.aspx", StringComparison.OrdinalIgnoreCase)>0)
            {
                var typeValue = (HttpContext.Current.Request["type"] ?? "common").ToLower();
                CurrentPage = "settings_" + typeValue;
            }
            else if (currentPath.IndexOf("Cases.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "cases";
            }
            else if (currentPath.IndexOf("Deals.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "deals";
            }
            else if (currentPath.IndexOf("Tasks.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "tasks";
            }
            else if (currentPath.IndexOf("Help.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "help";
            }
            else if (currentPath.IndexOf("Invoices.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "invoices";
            }
            else if (currentPath.IndexOf("MailViewer.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "contacts";
            }
            else if (currentPath.IndexOf("Calls.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                CurrentPage = "calls";
            }
            else if (currentPath.IndexOf("Reports.aspx", StringComparison.OrdinalIgnoreCase) > 0)
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