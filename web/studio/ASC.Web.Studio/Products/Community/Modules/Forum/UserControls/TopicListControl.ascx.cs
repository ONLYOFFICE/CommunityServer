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
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using ASC.Core;
using ASC.Forum;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;
using System.Globalization;
using System.Web;

namespace ASC.Web.UserControls.Forum
{
    public partial class TopicListControl : UserControl
    {
        public int ThreadID { get; set; }

        public List<Topic> Topics { get; private set; }

        public Guid SettingsID { get; set; }

        public int PageSize
        {
            get { return ViewState["PageSize"] != null ? Convert.ToInt32(ViewState["PageSize"]) : 20; }
            set { ViewState["PageSize"] = value; }
        }

        public long TopicPagesCount { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {            
            var settings = ForumManager.GetSettings(SettingsID);
            PageSize = string.IsNullOrEmpty(Request["size"]) ? 20 : Convert.ToInt32(Request["size"]);
            //var pageSize = PageSize;            
            Topics = new List<Topic>();
           
            if (ThreadID == 0)
                Response.Redirect(settings.StartPageAbsolutePath);

            int currentPageNumber = 0;
            if (!String.IsNullOrEmpty(Request["p"]))
            {
                try
                {
                    currentPageNumber = Convert.ToInt32(Request["p"]);                    
                }
                catch { 
                    currentPageNumber = 0;                    
                }
            }
            if (currentPageNumber <= 0)
                currentPageNumber = 1;

            int topicCountInThread = 0;
            Topics = ForumDataProvider.GetTopics(TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID, ThreadID, currentPageNumber, PageSize, out topicCountInThread);

            ForumDataProvider.SetThreadVisit(TenantProvider.CurrentTenantID, ThreadID);

            int i = 0;
            foreach (Topic topic in Topics)
            {
                TopicControl topicControl = (TopicControl)LoadControl(settings.UserControlsVirtualPath+"/TopicControl.ascx");
                topicControl.Topic = topic;
                topicControl.SettingsID = SettingsID;
                topicControl.IsEven = (i % 2 == 0);
                this.topicListHolder.Controls.Add(topicControl);
                i++;
            }
            PageSize = string.IsNullOrEmpty(Request["size"]) ? 20 : Convert.ToInt32(Request["size"]);
            var pageSize = PageSize;            

            PageNavigator pageNavigator = new PageNavigator()
            {
                PageUrl = string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}?&f={1}&size={2}",
                        VirtualPathUtility.ToAbsolute("~/products/community/modules/forum/topics.aspx"),
                        ThreadID,
                        pageSize
                        ),
                //settings.LinkProvider.TopicList(ThreadID),
                CurrentPageNumber = currentPageNumber,
                EntryCountOnPage = pageSize, //settings.TopicCountOnPage,
                VisiblePageCount = 5,
                EntryCount = topicCountInThread
            };
            TopicPagesCount = pageNavigator.EntryCount;
            var pageCount = (int)(TopicPagesCount / pageSize + 1);
            if (pageCount < pageNavigator.CurrentPageNumber)
            {
                pageNavigator.CurrentPageNumber = pageCount;
            }
            
            bottomPageNavigatorHolder.Controls.Add(pageNavigator);
            InitScripts();
        }

        private void InitScripts()
        {
            var jsResource = new StringBuilder();
            jsResource.Append("jq('#tableForNavigation select').val(" + PageSize + ").change(function(evt) {changeCountOfRows(this.value);}).tlCombobox();");
            Page.RegisterInlineScript(jsResource.ToString(), true);
        }
    }
}