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
using System.Globalization;
using System.Text;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Community.News.Code;
using ASC.Web.Community.News.Code.DAO;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Community.News.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;
using FeedNS = ASC.Web.Community.News.Code;

namespace ASC.Web.Community.News
{
    public partial class Default : MainPage
    {
        //private BBCodeParser.Parser postParser = new BBCodeParser.Parser(CommonControlsConfigurer.SimpleTextConfig);

        private RequestInfo info;

        public RequestInfo Info
        {
            get { return info ?? (info = new RequestInfo(Request)); }
        }

        public int PageNumber
        {
            get { return ViewState["PageNumber"] != null ? Convert.ToInt32(ViewState["PageNumber"]) : 0; }
            set { ViewState["PageNumber"] = value; }
        }

        public int PageSize
        {
            get { return ViewState["PageSize"] != null ? Convert.ToInt32(ViewState["PageSize"]) : 20; }
            set { ViewState["PageSize"] = value; }
        }

        public long FeedsCount { get; set; }

        protected string EventTitle { get; set; }
        protected string StatusSubscribe { get; set; }

        protected Uri FeedItemUrlWithParam
        {
            get { return new Uri("~/Products/Community/Modules/News/EditNews.aspx?docID=" + Info.UserIdAttribute, UriKind.Relative); }

        }

        protected void Page_Load(object sender, EventArgs e)
        {            
            commentList.Visible = CommunitySecurity.CheckPermissions(NewsConst.Action_Comment);

            pgNavigator.EntryCount = 1;
            pgNavigator.EntryCountOnPage = 1;

            if (IsPostBack) return;

            var storage = FeedStorageFactory.Create();
            if (!string.IsNullOrEmpty(Request["docID"]))
            {
                long docID;
                if (long.TryParse(Request["docID"], out docID))
                {
                    //Show panel
                    ContentView.Visible = false;
                    FeedView.Visible = true;

                    var feed = storage.GetFeed(docID);
                    if (feed != null)
                    {

                        if (!feed.Readed)
                        {
                            storage.ReadFeed(feed.Id, SecurityContext.CurrentAccount.ID.ToString());
                        }
                        FeedViewCtrl.Feed = feed;
                        hdnField.Value = feed.Id.ToString(CultureInfo.CurrentCulture);
                        InitCommentsView(storage, feed);
                        FeedView.DataBind();
                        EventTitle = feed.Caption;
                        var subscriptionProvider = NewsNotifySource.Instance.GetSubscriptionProvider();
                        var amAsRecipient = (IDirectRecipient)NewsNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString());
                        var isSubsribedOnComments = subscriptionProvider.IsSubscribed(NewsConst.NewComment, amAsRecipient, feed.Id.ToString());

                        var SubscribeTopicLink = string.Format(CultureInfo.CurrentCulture,
                                                               string.Format(CultureInfo.CurrentCulture,
                                                                             "<a id=\"statusSubscribe\" class=\"follow-status " +
                                                                             (isSubsribedOnComments ? "subscribed" : "unsubscribed") +
                                                                             "\" title=\"{0}\" href=\"#\" onclick=\"SubscribeOnComments(this,'{1}','{2}','{3}');\"></a>",
                                                                             (isSubsribedOnComments ? NewsResource.UnsubscribeFromNewComments : NewsResource.SubscribeOnNewComments),
                                                                             feed.Id,
                                                                             NewsResource.SubscribeOnNewComments.ReplaceSingleQuote(),
                                                                             NewsResource.UnsubscribeFromNewComments.ReplaceSingleQuote()));

                        SubscribeLinkBlock.Text = SubscribeTopicLink;

                        Title = HeaderStringHelper.GetPageTitle((Master as NewsMaster).CurrentPageCaption ?? feed.Caption);
                    }
                    else
                    {
                        Response.Redirect(VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/"));
                        ContentView.Visible = true;
                        FeedView.Visible = false;
                        FeedRepeater.Visible = true;
                    }
                }
            }
            else
            {
                PageNumber = string.IsNullOrEmpty(Request["page"]) ? 1 : Convert.ToInt32(Request["page"]);
                PageSize = string.IsNullOrEmpty(Request["size"]) ? 20 : Convert.ToInt32(Request["size"]);
                LoadData();
            }
            InitScripts();
        }

        private void InitScripts()
        {
            var jsResource = new StringBuilder();
            jsResource.Append("jq('#tableForNavigation select').val(" + PageSize + ").change(function(evt) {changeCountOfRows(this.value);}).tlCombobox();");
            Page.RegisterInlineScript(jsResource.ToString(), true);
        }

        private void InitCommentsView(IFeedStorage storage, FeedNS.Feed feed)
        {
            IList<CommentInfo> comments = BuildCommentsList(storage.GetFeedComments(feed.Id));

            //AppendChildsComments(ref comments, storage.GetFeedComments(feed.Id));

            ConfigureComments(commentList, feed);
            commentList.Items = comments;
            commentList.TotalCount = GetCommentsCount(comments);
        }

        private static void ConfigureComments(CommentsList commentList, FeedNS.Feed feed)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);

            commentList.BehaviorID = "_commentsObj";
            commentList.FckDomainName = "news_comments";
            commentList.ModuleName = "news";

            commentList.ObjectID = feed != null ? feed.Id.ToString(CultureInfo.CurrentCulture) : "";
        }

        private static int GetCommentsCount(ICollection<CommentInfo> comments)
        {
            var count = comments.Count;
            foreach (var info in comments)
            {
                count += GetCommentsCount(info.CommentList);
            }
            return count;
        }

        private static CommentInfo GetCommentInfo(FeedComment comment)
        {
            var info = new CommentInfo
                {
                    CommentID = comment.Id.ToString(CultureInfo.CurrentCulture),
                    UserID = new Guid(comment.Creator),
                    TimeStamp = comment.Date,
                    TimeStampStr = comment.Date.Ago(),
                    IsRead = true,
                    Inactive = comment.Inactive,
                    CommentBody = HtmlUtility.GetFull(comment.Comment),
                    UserFullName = DisplayUserSettings.GetFullUserName(new Guid(comment.Creator)),
                    UserProfileLink = CommonLinkUtility.GetUserProfile(comment.Creator),
                    UserAvatarPath = UserPhotoManager.GetBigPhotoURL(new Guid(comment.Creator)),
                    IsEditPermissions = CommunitySecurity.CheckPermissions(comment, NewsConst.Action_Edit),
                    IsResponsePermissions = CommunitySecurity.CheckPermissions(NewsConst.Action_Comment),
                    UserPost = CoreContext.UserManager.GetUsers((new Guid(comment.Creator))).Title
                };

            return info;
        }

        private static List<CommentInfo> BuildCommentsList(List<FeedComment> loaded)
        {
            return BuildCommentsList(loaded, 0);
        }

        private static List<CommentInfo> BuildCommentsList(List<FeedComment> loaded, long parentId)
        {
            var result = new List<CommentInfo>();
            foreach (var comment in FeedComment.SelectChildLevel(parentId, loaded))
            {
                var info = GetCommentInfo(comment);
                info.CommentList = BuildCommentsList(loaded, comment.Id);

                result.Add(info);
            }
            return result;
        }

        private void LoadData()
        {
            var storage = FeedStorageFactory.Create();
            var feedType = FeedType.All;

            if (!string.IsNullOrEmpty(Request["type"]))
            {
                feedType = (FeedType)Enum.Parse(typeof(FeedType), Request["type"], true);
                var feedTypeInfo = FeedTypeInfo.FromFeedType(feedType);
                Title = HeaderStringHelper.GetPageTitle((Master as NewsMaster).CurrentPageCaption ?? feedTypeInfo.TypeName);
            }
            else
            {
                Title = HeaderStringHelper.GetPageTitle((Master as NewsMaster).CurrentPageCaption ?? NewsResource.NewsBreadCrumbs);
            }

            var feedsCount = !string.IsNullOrEmpty(Request["search"]) ? storage.SearchFeedsCount(Request["search"], feedType, Info.UserId) : storage.GetFeedsCount(feedType, Info.UserId);

            FeedsCount = feedsCount;

            if (feedsCount == 0)
            {
                FeedRepeater.Visible = false;
                MessageShow.Visible = true;

                string buttonLink;
                string buttonName;

                var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var emptyScreenControl = new EmptyScreenControl { Describe = currentUser.IsVisitor() ? NewsResource.EmptyScreenTextVisitor : NewsResource.EmptyScreenText };

                switch (feedType)
                {
                    case FeedType.News:
                        emptyScreenControl.ImgSrc = WebImageSupplier.GetAbsoluteWebPath("150x_news.png", NewsConst.ModuleId);
                        emptyScreenControl.Header = NewsResource.EmptyScreenNewsCaption;
                        buttonLink = FeedUrls.EditNewsUrl;
                        buttonName = NewsResource.EmptyScreenNewsLink;
                        break;
                    case FeedType.Order:
                        emptyScreenControl.ImgSrc = WebImageSupplier.GetAbsoluteWebPath("150x_order.png", NewsConst.ModuleId);
                        emptyScreenControl.Header = NewsResource.EmptyScreenOrdersCaption;
                        buttonLink = FeedUrls.EditOrderUrl;
                        buttonName = NewsResource.EmptyScreenOrderLink;
                        break;
                    case FeedType.Advert:
                        emptyScreenControl.ImgSrc = WebImageSupplier.GetAbsoluteWebPath("150x_advert.png", NewsConst.ModuleId);
                        emptyScreenControl.Header = NewsResource.EmptyScreenAdvertsCaption;
                        buttonLink = FeedUrls.EditAdvertUrl;
                        buttonName = NewsResource.EmptyScreenAdvertLink;
                        break;
                    case FeedType.Poll:
                        emptyScreenControl.ImgSrc = WebImageSupplier.GetAbsoluteWebPath("150x_poll.png", NewsConst.ModuleId);
                        emptyScreenControl.Header = NewsResource.EmptyScreenPollsCaption;
                        buttonLink = FeedUrls.EditPollUrl;
                        buttonName = NewsResource.EmptyScreenPollLink;
                        break;
                    default:
                        emptyScreenControl.ImgSrc = WebImageSupplier.GetAbsoluteWebPath("150x_newslogo.png", NewsConst.ModuleId);
                        emptyScreenControl.Header = NewsResource.EmptyScreenEventsCaption;
                        buttonLink = FeedUrls.EditNewsUrl;
                        buttonName = NewsResource.EmptyScreenEventLink;
                        break;
                }

                if (CommunitySecurity.CheckPermissions(NewsConst.Action_Add) && String.IsNullOrEmpty(Request["uid"]) && String.IsNullOrEmpty(Request["search"]))
                    emptyScreenControl.ButtonHTML = String.Format("<a class='link underline blue plus' href='{0}'>{1}</a>", buttonLink, buttonName);


                MessageShow.Controls.Add(emptyScreenControl);
            }
            else
            {
                var pageSize = PageSize;
                var pageCount = (int)(feedsCount/pageSize + 1);
                if (pageCount < PageNumber) PageNumber = pageCount;

                var feeds = !string.IsNullOrEmpty(Request["search"]) ?
                                storage.SearchFeeds(Request["search"], feedType, Info.UserId, pageSize, (PageNumber - 1)*pageSize) :
                                storage.GetFeeds(feedType, Info.UserId, pageSize, (PageNumber - 1)*pageSize);

                pgNavigator.EntryCountOnPage = pageSize;
                pgNavigator.EntryCount = 0 < pageCount ? (int)feedsCount : pageSize;
                pgNavigator.CurrentPageNumber = PageNumber;

                pgNavigator.ParamName = "page";
                if (!string.IsNullOrEmpty(Request["search"]))
                {
                    pgNavigator.PageUrl = string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}?search={1}&size={2}",
                        VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/"),
                        Request["search"],
                        pageSize
                        );
                }
                else
                {
                    pgNavigator.PageUrl = string.IsNullOrEmpty(Request["type"]) ?
                                              string.Format(
                                                  CultureInfo.CurrentCulture,
                                                  "{0}?{1}&size={2}",
                                                  VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/"),
                                                  (string.IsNullOrEmpty(Info.UserIdAttribute) ? string.Empty : "?" + Info.UserIdAttribute.Substring(1)),
                                                  pageSize
                                                  ) :
                                              string.Format(
                                                  CultureInfo.CurrentCulture,
                                                  "{0}?type={1}{2}&size={3}",
                                                  VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/"),
                                                  Request["type"],
                                                  Info.UserIdAttribute,
                                                  pageSize);
                }
                FeedRepeater.DataSource = feeds;
                FeedRepeater.DataBind();
            }
        }
    }
}