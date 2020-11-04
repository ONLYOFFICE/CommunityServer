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
        public System.Web.UI.WebControls.PlaceHolder PaggingHolder { get; set; }

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
                                            VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Forum/Posts.aspx"),
                                            Topic.ID,
                                            pageSize
                                            ),
                                        //_settings.LinkProvider.PostList(Topic.ID),
                                        CurrentPageNumber = currentPageNumber,
                                        EntryCountOnPage = pageSize,
                                        VisiblePageCount = 5,
                                        EntryCount = postCountInTopic
                                    };

            var pagingControl = (PagingControl)LoadControl(_settings.UserControlsVirtualPath + "/PagingControl.ascx");
            pagingControl.Count = PostPagesCount;
            pagingControl.PageNavigator = pageNavigator;
            PaggingHolder.Controls.Add(pagingControl);

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
