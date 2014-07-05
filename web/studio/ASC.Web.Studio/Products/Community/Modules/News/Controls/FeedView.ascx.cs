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
using System.Globalization;
using ASC.Web.Studio.UserControls.Common.PollForm;
using ASC.Web.Studio.Utility.HtmlUtility;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Community.News.Code;
using ASC.Web.Community.News.Code.DAO;
using ASC.Web.Community.News.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Studio.Utility;
using FeedNS = ASC.Web.Community.News.Code;

namespace ASC.Web.Community.News.Controls
{
    [AjaxNamespace("FeedView")]
    public partial class FeedView : System.Web.UI.UserControl
    {
        protected Guid RequestedUserId
        {
            get
            {
                var result = Guid.Empty;
                try
                {
                    result = new Guid(Request["uid"]);
                }
                catch
                {
                }

                return result;
            }
        }

        protected string UserIdAttribute
        {
            get
            {
                return RequestedUserId.Equals(Guid.Empty)
                           ? string.Empty
                           : string.Format(CultureInfo.CurrentCulture, "&uid={0}", RequestedUserId);
            }
        }

        public FeedNS.Feed Feed { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(typeof(FeedView), this.Page);
        }


        public override void DataBind()
        {
            base.DataBind();

            if (Feed == null) return;

            newsText.Text = HtmlUtility.GetFull(Feed.Text, CommunityProduct.ID);
            Date.Text = Feed.Date.Ago();

            EditorButtons.Visible = CommunitySecurity.CheckPermissions(Feed, NewsConst.Action_Edit);

            if (EditorButtons.Visible)
            {
                EditorButtons.Text = string.Format(CultureInfo.CurrentCulture, "{0}{1}",
                                                   string.Format(CultureInfo.CurrentCulture, "<li><a class=\"dropdown-item\" href=\"{0}?docID={1}{3}\">{2}</a></li>", GetEditUrl(), Feed.Id, NewsResource.EditButton, UserIdAttribute),
                                                   string.Format(CultureInfo.CurrentCulture, "<li><a class=\"dropdown-item\" href=\"javascript:;\" onclick=\"javascript:if(window.confirm('{0}'))FeedView.Remove('{1}',callbackRemove);\">{2}</a></li>", NewsResource.ConfirmRemoveMessage, Feed.Id, NewsResource.DeleteButton));
            }

            profileLink.Text = CoreContext.UserManager.GetUsers(new Guid(Feed.Creator)).RenderCustomProfileLink(CommunityProduct.ID, "", "linkMedium");

            if (Feed is FeedPoll)
            {
                var poll = (FeedPoll)Feed;
                var isMakeVote = TenantUtil.DateTimeNow() <= poll.EndDate && !poll.IsUserVote(SecurityContext.CurrentAccount.ID.ToString());

                var pollForm = new PollForm
                    {
                        VoteHandlerType = typeof(PollVoteHandler),
                        Answered = !isMakeVote,
                        Name = poll.Caption,
                        PollID = poll.Id.ToString(CultureInfo.CurrentCulture),
                        Singleton = (poll.PollType == FeedPollType.SimpleAnswer),
                        AdditionalParams = poll.Id.ToString(CultureInfo.CurrentCulture)
                    };

                foreach (var variant in poll.Variants)
                {
                    pollForm.AnswerVariants.Add(new PollForm.AnswerViarint()
                        {
                            ID = variant.ID.ToString(CultureInfo.CurrentCulture),
                            Name = variant.Name,
                            VoteCount = poll.GetVariantVoteCount(variant.ID)
                        });
                }
                pollHolder.Controls.Add(pollForm);
            }
            else
            {
                pollHolder.Visible = false;
                newsText.Visible = true;
            }
        }

        private string GetEditUrl()
        {
            return Feed is FeedPoll ? FeedUrls.EditPollUrl : FeedUrls.EditNewsUrl;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse Remove(string id)
        {
            try
            {
                var resp = new AjaxResponse { rs1 = "0" };
                if (!string.IsNullOrEmpty(id))
                {
                    var feedId = Convert.ToInt64(id);
                    var storage = FeedStorageFactory.Create();

                    var feed = storage.GetFeed(feedId);
                    CommunitySecurity.DemandPermissions(feed, NewsConst.Action_Edit);

                    foreach (var comment in storage.GetFeedComments(feedId))
                    {
                        CommonControlsConfigurer.FCKUploadsRemoveForItem("news_comments", comment.Id.ToString());
                    }

                    storage.RemoveFeed(feedId);
                    CommonControlsConfigurer.FCKUploadsRemoveForItem("news", id);

                    resp.rs1 = id;
                    resp.rs2 = NewsResource.FeedDeleted;
                }
                return resp;
            }
            catch (Exception err)
            {
                return new AjaxResponse { rs1 = "1", rs2 = err.Message, };
            }
        }
    }
}