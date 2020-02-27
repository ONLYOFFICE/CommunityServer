/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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

            newsText.Text = HtmlUtility.GetFull(Feed.Text);
            Date.Text = Feed.Date.Ago();

            EditorButtons.Visible = CommunitySecurity.CheckPermissions(Feed, NewsConst.Action_Edit);

            if (EditorButtons.Visible)
            {
                EditorButtons.Text = string.Format(CultureInfo.CurrentCulture, "{0}{1}",
                                                   string.Format(CultureInfo.CurrentCulture, "<li><a class=\"dropdown-item\" href=\"{0}?docID={1}{3}\">{2}</a></li>", GetEditUrl(), Feed.Id, NewsResource.EditButton, UserIdAttribute),
                                                   string.Format(CultureInfo.CurrentCulture, "<li><a class=\"dropdown-item\" href=\"javascript:;\" onclick=\"javascript:if(window.confirm('{0}'))FeedView.Remove('{1}',callbackRemove);\">{2}</a></li>", NewsResource.ConfirmRemoveMessage, Feed.Id, NewsResource.DeleteButton));
            }

            profileLink.Text = CoreContext.UserManager.GetUsers(new Guid(Feed.Creator)).RenderCustomProfileLink("", "linkMedium");

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

                    storage.RemoveFeed(feed);
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