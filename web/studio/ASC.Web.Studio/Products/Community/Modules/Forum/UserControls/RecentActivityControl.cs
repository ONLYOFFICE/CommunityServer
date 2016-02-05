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
            _settings = Community.Forum.ForumManager.Settings;
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
                    sb.Append(topic.RecentPostAuthor.RenderCustomProfileLink("describe-text", "link gray"));

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
