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
using System.Web;
using ASC.Forum;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.Community.Forum
{
    public partial class Search : MainPage
    {
        protected bool _isFind;
        private bool _isFindByTag;
        private int _tagID;

        protected void Page_Load(object sender, EventArgs e)
        {
            ForumManager.Instance.SetCurrentPage(ForumPage.Search);

            var currentPageNumber = 0;
            if (!String.IsNullOrEmpty(Request["p"]))
            {
                try
                {
                    currentPageNumber = Convert.ToInt32(Request["p"]);
                }
                catch
                {
                    currentPageNumber = 0;
                }
            }
            if (currentPageNumber <= 0)
                currentPageNumber = 1;

            var findTopicList = new List<Topic>();
            var topicCount = 0;

            var searchText = "";
            _tagID = 0;
            _isFindByTag = false;

            var tagName = "";

            if (!String.IsNullOrEmpty(Request["tag"]))
            {
                _isFindByTag = true;
                try
                {
                    _tagID = Convert.ToInt32(Request["tag"]);
                }
                catch
                {
                    _tagID = 0;
                }

                findTopicList = ForumDataProvider.SearchTopicsByTag(TenantProvider.CurrentTenantID, _tagID, currentPageNumber, ForumManager.Settings.TopicCountOnPage, out topicCount);

            }
            else if (!String.IsNullOrEmpty(Request["search"]))
            {
                searchText = Request["search"];
                findTopicList = ForumDataProvider.SearchTopicsByText(TenantProvider.CurrentTenantID, searchText, currentPageNumber, ForumManager.Settings.TopicCountOnPage, out topicCount);
            }

            if (findTopicList.Count > 0)
            {
                _isFind = true;

                var i = 0;
                foreach (var topic in findTopicList)
                {
                    if (i == 0)
                    {
                        foreach (var tag in topic.Tags)
                        {
                            if (tag.ID == _tagID)
                            {
                                tagName = tag.Name;
                                break;
                            }
                        }
                    }

                    var topicControl = (TopicControl)LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/TopicControl.ascx");
                    topicControl.SettingsID = ForumManager.Settings.ID;
                    topicControl.Topic = topic;
                    topicControl.IsShowThreadName = true;
                    topicControl.IsEven = (i%2 == 0);
                    topicListHolder.Controls.Add(topicControl);
                    i++;
                }

                var pageNavigator = new PageNavigator
                    {
                        CurrentPageNumber = currentPageNumber,
                        EntryCountOnPage = ForumManager.Settings.TopicCountOnPage,
                        VisiblePageCount = 5,
                        EntryCount = topicCount
                    };
                if (_isFindByTag)
                    pageNavigator.PageUrl = _isFindByTag
                                                ? "Search.aspx?tag=" + _tagID.ToString()
                                                : "Search.aspx?search=" + HttpUtility.UrlEncode(searchText, Encoding.UTF8);

                bottomPageNavigatorHolder.Controls.Add(pageNavigator);
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

            (Master as ForumMasterPage).CurrentPageCaption = HeaderStringHelper.GetHTMLSearchHeader(_isFindByTag ? tagName : searchText);

            Title = HeaderStringHelper.GetPageTitle((Master as ForumMasterPage).CurrentPageCaption);
        }
    }
}