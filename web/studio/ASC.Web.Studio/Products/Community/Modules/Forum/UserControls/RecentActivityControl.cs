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
using System.Web.UI.WebControls;
using ASC.Core.Users;
using ASC.Forum;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.UserControls.Forum
{   
    public class RecentActivityControl: WebControl
    {   
        public Guid SettingsID { get; set; }
        public int MaxTopicCount { get; set; }
        public bool AutoInitView { get; set; }
        
        private Settings _settings = null;

        public List<Topic> TopicList { get; set; }

        public RecentActivityControl()
        {
            this.MaxTopicCount = 8;
            AutoInitView = true;
            TopicList = new List<Topic>();
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (AutoInitView)
                InitView();
            
        }

        public void InitView()
        {
            _settings = ForumManager.GetSettings(SettingsID);
            TopicList = ForumDataProvider.GetLastUpdateTopics(TenantProvider.CurrentTenantID, MaxTopicCount);
        }    

        protected override void RenderContents(System.Web.UI.HtmlTextWriter writer)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var topic in TopicList)
            {
                if (topic.RecentPostID != 0)
                {
                    //poster
                    sb.Append("<div class='clearFix' style='margin-bottom:20px;'>");
                    sb.Append(topic.RecentPostAuthor.RenderCustomProfileLink(_settings.ProductID, "describe-text", "link gray"));

                    //topic
                    sb.Append("<div style='margin-top:5px;'>");
                    sb.Append("<a href='" + _settings.LinkProvider.RecentPost(topic.RecentPostID, topic.ID, topic.PostCount) + "'>" + HttpUtility.HtmlEncode(topic.Title) + "</a>");
                    sb.Append("</div>");


                    //date
                    sb.Append("<div style='margin-top:5px;'>");
                    sb.Append(DateTimeService.DateTime2StringPostStyle(topic.RecentPostCreateDate));
                    sb.Append("</div>");

                    sb.Append("</div>");
                }

            }
            
            writer.Write(sb.ToString());
        }
    }
}
