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
                                                ? "search.aspx?tag=" + _tagID.ToString()
                                                : "search.aspx?search=" + HttpUtility.UrlEncode(searchText, Encoding.UTF8);

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