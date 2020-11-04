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

        public System.Web.UI.WebControls.PlaceHolder PaggingHolder { get; set; }

        public int PageSize
        {
            get { return ViewState["PageSize"] != null ? Convert.ToInt32(ViewState["PageSize"]) : 20; }
            set { ViewState["PageSize"] = value; }
        }

        public long TopicPagesCount { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var settings = Community.Forum.ForumManager.Settings;
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
                        VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Forum/Topics.aspx"),
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

            var pagingControl = (PagingControl)LoadControl(settings.UserControlsVirtualPath + "/PagingControl.ascx");
            pagingControl.Count = TopicPagesCount;
            pagingControl.PageNavigator = pageNavigator;
            PaggingHolder.Controls.Add(pagingControl);

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