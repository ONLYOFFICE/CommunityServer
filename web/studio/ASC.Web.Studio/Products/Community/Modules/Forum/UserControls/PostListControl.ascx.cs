/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Web.UI;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.Common.PollForm;
using AjaxPro;
using ASC.Forum;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;
using ASC.Core;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Web;

namespace ASC.Web.UserControls.Forum
{
    internal class PollVoteHandler : IVoteHandler  
    {
        #region IVoteHandler Members

        public bool VoteCallback(string pollID, List<string> selectedVariantIDs, string additionalParams, out string errorMessage)
        {   
            errorMessage = "";
            int idQuestion = Convert.ToInt32(additionalParams.Split(',')[1]);
            var _forumManager = Community.Forum.ForumManager.Settings.ForumManager;
                    

            var variantIDs = new List<int>(0);
            foreach (var id in selectedVariantIDs)
            {
                if (!string.IsNullOrEmpty(id))
                    variantIDs.Add(Convert.ToInt32(id));
            }

            var q = ForumDataProvider.GetPollByID(TenantProvider.CurrentTenantID, idQuestion);
            if (q == null
                || !_forumManager.ValidateAccessSecurityAction(ForumAction.PollVote, q)
                || ForumDataProvider.IsUserVote(TenantProvider.CurrentTenantID, idQuestion,SecurityContext.CurrentAccount.ID))
            {
                errorMessage = Resources.ForumUCResource.ErrorAccessDenied;
                return false;
            }

            try
            {
                ForumDataProvider.PollVote(TenantProvider.CurrentTenantID, idQuestion, variantIDs);
            }
            catch (Exception e)
            {
                errorMessage = e.Message.HtmlEncode();
                return false;
            }
            return true;
        }

        #endregion

    }

    [AjaxNamespace("PostListControl")]
    public partial class PostListControl : UserControl
    {  
        public Guid SettingsID { get; set; }
        public Topic Topic { get; set; }
        public long PostPagesCount { get; set; }
        public int PostPageSize 
        {
            get { return ViewState["PageSize"] != null ? Convert.ToInt32(ViewState["PageSize"]) : 20; }
            set { ViewState["PageSize"] = value; }
        }

        protected Settings _settings;
        private ForumManager _forumManager;

        protected void Page_Load(object sender, EventArgs e)
        {
            _settings = Community.Forum.ForumManager.Settings;
            _forumManager = _settings.ForumManager;
            PostPageSize = string.IsNullOrEmpty(Request["size"]) ? 20 : Convert.ToInt32(Request["size"]);

            if (Topic == null) Response.Redirect(_settings.StartPageAbsolutePath);

            if ((new Thread { ID = Topic.ThreadID }).Visible == false)
                Response.Redirect(_settings.StartPageAbsolutePath);
            

            int currentPageNumber = 0;
            if (!String.IsNullOrEmpty(Request["p"]))
            {
                try
                {
                    currentPageNumber = Convert.ToInt32(Request["p"]);
                }
                catch { currentPageNumber = 0; }
            }
            if (currentPageNumber <= 0)
                currentPageNumber = 1;           
            
            int postCountInTopic;
            var posts = ForumDataProvider.GetPosts(TenantProvider.CurrentTenantID, Topic.ID, currentPageNumber, PostPageSize, out postCountInTopic);
            
            var postId = 0;
            if (!string.IsNullOrEmpty(Request["post"]))
            {
                try
                {
                    postId = Convert.ToInt32(Request["post"]);
                }
                catch { postId = 0; }
            }

            if (postId != 0)
            {
                var allposts = ForumDataProvider.GetPostIDs(TenantProvider.CurrentTenantID, Topic.ID);
                var idx = -1;
                for (var j = 0; j < allposts.Count; j++)
                {
                    if (allposts[j] != postId) continue;

                    idx = j;
                    break;
                }
                if (idx != -1)
                {
                    var page = idx / 20 + 1;
                    Response.Redirect("posts.aspx?t=" + Topic.ID + "&size=20&p=" + page + "#" + postId);
                }
            }

            PostPagesCount = postCountInTopic;
            var pageSize = PostPageSize;
            var pageNavigator = new PageNavigator
                                    {
                                        PageUrl = string.Format(
                                            CultureInfo.CurrentCulture,
                                            "{0}?&t={1}&size={2}",
                                            VirtualPathUtility.ToAbsolute("~/products/community/modules/forum/posts.aspx"),
                                            Topic.ID,
                                            pageSize
                                            ),
                                        //_settings.LinkProvider.PostList(Topic.ID),
                                        CurrentPageNumber = currentPageNumber,
                                        EntryCountOnPage = pageSize,
                                        VisiblePageCount = 5,
                                        EntryCount = postCountInTopic
                                    };
                     

            bottomPageNavigatorHolder.Controls.Add(pageNavigator);

            var i = 0;
            foreach (var post in posts)
            {
                var postControl = (PostControl) LoadControl(_settings.UserControlsVirtualPath + "/PostControl.ascx");
                postControl.Post = post;
                postControl.IsEven = (i%2==0);
                postControl.SettingsID = SettingsID;
                postControl.CurrentPageNumber = currentPageNumber;
				postControl.PostsCount = Topic.PostCount;
                postListHolder.Controls.Add(postControl);
                i++;
            }

            ForumDataProvider.SetTopicVisit(Topic);
            InitScripts();
            if (Topic.Type != TopicType.Poll) return;
            
            var q = ForumDataProvider.GetPollByID(TenantProvider.CurrentTenantID, Topic.QuestionID);
            if (q == null) return;

            var isVote = ForumDataProvider.IsUserVote(TenantProvider.CurrentTenantID, q.ID, SecurityContext.CurrentAccount.ID);

            var pollForm = new PollForm
                               {
                                   VoteHandlerType = typeof (PollVoteHandler),
                                   Answered = isVote || Topic.Closed || !_forumManager.ValidateAccessSecurityAction(ForumAction.PollVote, q),
                                   Name = q.Name,
                                   PollID = q.ID.ToString(),
                                   Singleton = (q.Type == QuestionType.OneAnswer),
                                   AdditionalParams = _settings.ID.ToString() + "," + q.ID.ToString()
                               };


            foreach (var variant in q.AnswerVariants)
            {
                pollForm.AnswerVariants.Add(new PollForm.AnswerViarint
                                                {
                                                    ID = variant.ID.ToString(),
                                                    Name = variant.Name,
                                                    VoteCount = variant.AnswerCount
                                                });
            }


            pollHolder.Controls.Add(new Literal {Text = "<div style='position:relative; padding-left:20px; margin-bottom:15px;'>"});
            pollHolder.Controls.Add(pollForm);
            pollHolder.Controls.Add(new Literal {Text = "</div>"});
            
        }

        private void InitScripts()
        {
            var jsResource = new StringBuilder();
            jsResource.Append("jq('#tableForNavigation select').val(" + PostPageSize + ").change(function(evt) {changePostCountOfRows(this.value);}).tlCombobox();");
            Page.RegisterInlineScript(jsResource.ToString(), true);
        }
    }
}
