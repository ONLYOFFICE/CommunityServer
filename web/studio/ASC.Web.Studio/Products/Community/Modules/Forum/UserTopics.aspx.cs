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
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Forum;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.Community.Forum
{
    [AjaxNamespace("SearchResult")]
    public partial class UserTopics : MainPage
    {
        protected bool _isFind;
        private Guid _userID;

        protected void Page_Load(object sender, EventArgs e)
        {
            ForumManager.Instance.SetCurrentPage(ForumPage.Search);

            int currentPageNumber;
            if (!int.TryParse(Request["p"], out currentPageNumber))
                currentPageNumber = 1;

            if (currentPageNumber <= 0)
                currentPageNumber = 1;

            var findTopicList = new List<Topic>();
            var topicCount = 0;

            if (!String.IsNullOrEmpty(Request["uid"]))
            {
                try
                {
                    _userID = new Guid(Request["uid"]);
                }
                catch
                {
                    _userID = Guid.Empty;
                }

                if (_userID != Guid.Empty)
                {
                    findTopicList = ForumDataProvider.SearchTopicsByUser(TenantProvider.CurrentTenantID, _userID, currentPageNumber, ForumManager.Settings.TopicCountOnPage, out topicCount);
                }
            }

            if (findTopicList.Count > 0)
            {
                _isFind = true;

                var i = 0;
                foreach (var topic in findTopicList)
                {
                    var topicControl = (TopicControl)LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/TopicControl.ascx");
                    topicControl.SettingsID = ForumManager.Settings.ID;
                    topicControl.Topic = topic;
                    topicControl.IsShowThreadName = true;
                    topicControl.IsEven = (i%2 == 0);
                    topicListHolder.Controls.Add(topicControl);
                    i++;
                }

                #region navigators

                var pageNavigator = new PageNavigator
                    {
                        CurrentPageNumber = currentPageNumber,
                        EntryCountOnPage = ForumManager.Settings.TopicCountOnPage,
                        VisiblePageCount = 5,
                        EntryCount = topicCount,
                        PageUrl = "usertopics.aspx?uid=" + _userID.ToString()
                    };

                bottomPageNavigatorHolder.Controls.Add(pageNavigator);

                #endregion
            }
            else
            {
                var emptyScreenControl = new EmptyScreenControl
                    {
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("forums_icon.png", ForumManager.Settings.ModuleID),
                        Header = Resources.ForumResource.EmptyScreenSearchCaption,
                        Describe = Resources.ForumResource.EmptyScreenSearchText,
                    };

                topicListHolder.Controls.Add(emptyScreenControl);
            }

            //bread crumbs
            (Master as ForumMasterPage).CurrentPageCaption = CoreContext.UserManager.GetUsers(_userID).DisplayUserName(false);

            Title = HeaderStringHelper.GetPageTitle((Master as ForumMasterPage).CurrentPageCaption);
        }
    }
}