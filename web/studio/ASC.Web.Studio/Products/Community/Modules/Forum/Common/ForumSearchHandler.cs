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
using System.Linq;
using System.Web;
using ASC.Forum;
using ASC.Web.Community.Product;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.Forum
{
    public class ForumSearchHandler : BaseSearchHandlerEx
    {
        public override SearchResultItem[] Search(string text)
        {
            int topicCount;
            var findTopicList = ForumDataProvider.SearchTopicsByText(TenantProvider.CurrentTenantID, text, 1, -1, out topicCount);

            return findTopicList.Select(topic => new SearchResultItem
                {
                    Name = topic.Title,
                    Description = String.Format(Resources.ForumResource.FindTopicDescription, topic.ThreadTitle),
                    URL = VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/Posts.aspx") + "?t=" + topic.ID,
                    Date = topic.CreateDate
                }).ToArray();
        }

        public override ImageOptions Logo
        {
            get { return new ImageOptions { ImageFileName = "forum_mini_icon.png", PartID = ForumManager.ModuleID }; }
        }

        public override string SearchName
        {
            get { return Resources.ForumResource.SearchDefaultString; }
        }

        public override Guid ModuleID
        {
            get { return ForumManager.ModuleID; }
        }

        public override Guid ProductID
        {
            get { return CommunityProduct.ID; }
        }

        public override IItemControl Control
        {
            get { return new ResultsView(); }
        }
    }
}